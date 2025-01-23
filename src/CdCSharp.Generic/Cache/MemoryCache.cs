using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CdCSharp.Generic.Cache;

/// <summary>
/// Implements <see cref="IMemoryCache"/> using a dictionary to
/// store its entries.
/// </summary>
public class MemoryCache : IMemoryCache
{
    private readonly MemoryCacheOptions _options;

    private readonly List<WeakReference<Stats>>? _allStats;
    private readonly Stats? _accumulatedStats;
    private readonly ThreadLocal<Stats>? _stats;
    private CoherentState _coherentState;
    private bool _disposed;
    private DateTime _lastExpirationScan;
    private readonly ConcurrentQueue<CacheEntry> _expirationQueue = new();
    private readonly SemaphoreSlim _compactionLock = new(1);
    private readonly int _batchSize = 100;
    private readonly TimeSpan _batchProcessDelay = TimeSpan.FromMilliseconds(100);
    private volatile bool _processingExpiredItems;

    /// <summary>
    /// Creates a new <see cref="MemoryCache"/> instance.
    /// </summary>
    /// <param name="optionsAccessor">The options of the cache.</param>
    public MemoryCache(Action<MemoryCacheOptions>? optionsAccessor = null)
    {
        _options = optionsAccessor != null ? ConfigureOptions(optionsAccessor) : new MemoryCacheOptions();

        _coherentState = new CoherentState();

        if (_options.TrackStatistics)
        {
            _allStats = [];
            _accumulatedStats = new Stats();
            _stats = new ThreadLocal<Stats>(() => new Stats(this));
        }

        _lastExpirationScan = UtcNow;
        TrackLinkedCacheEntries = _options.TrackLinkedCacheEntries; // we store the setting now so it's consistent for entire MemoryCache lifetime
    }
    private MemoryCacheOptions ConfigureOptions(Action<MemoryCacheOptions> optionsAccessor)
    {
        MemoryCacheOptions options = new();
        optionsAccessor(options);
        return options;
    }
    private DateTime UtcNow => _options.Clock?.UtcNow.UtcDateTime ?? DateTime.UtcNow;

    /// <summary>
    /// Cleans up the background collection events.
    /// </summary>
    ~MemoryCache() => Dispose(false);

    /// <summary>
    /// Gets the count of the current entries for diagnostic purposes.
    /// </summary>
    public int Count => _coherentState.Count;

    /// <summary>
    /// Gets an enumerable of the all the keys in the <see cref="MemoryCache"/>.
    /// </summary>
    public IEnumerable<object> Keys
    {
        get
        {
            foreach (KeyValuePair<object, CacheEntry> pairs in _coherentState._entries)
            {
                yield return pairs.Key;
            }
        }
    }

    /// <summary>
    /// Internal accessor for Size for testing only.
    ///
    /// Note that this is only eventually consistent with the contents of the collection.
    /// See comment on <see cref="CoherentState"/>.
    /// </summary>
    internal long Size => _coherentState.Size;

    internal bool TrackLinkedCacheEntries { get; }

    /// <inheritdoc />
    public ICacheEntry CreateEntry(object key)
    {
        CheckDisposed();
        ValidateCacheKey(key);

        return new CacheEntry(key, this);
    }

    internal void SetEntry(CacheEntry entry)
    {
        if (_disposed)
        {
            // No-op instead of throwing since this is called during CacheEntry.Dispose
            return;
        }

        if (_options.HasSizeLimit && entry.Size < 0)
        {
            throw new InvalidOperationException("CacheEntryHasEmptySize");
        }

        DateTime utcNow = UtcNow;

        // Applying the option's absolute expiration only if it's not already smaller.
        // This can be the case if a dependent cache entry has a smaller value, and
        // it was set by cascading it to its parent.
        if (entry.AbsoluteExpirationRelativeToNow.Ticks > 0)
        {
            long absoluteExpiration = (utcNow + entry.AbsoluteExpirationRelativeToNow).Ticks;
            if ((ulong)absoluteExpiration < (ulong)entry.AbsoluteExpirationTicks)
            {
                entry.AbsoluteExpirationTicks = absoluteExpiration;
            }
        }

        // Initialize the last access timestamp at the time the entry is added
        entry.LastAccessed = utcNow;

        CoherentState coherentState = _coherentState; // Clear() can update the reference in the meantime
        if (coherentState._entries.TryGetValue(entry.Key, out CacheEntry? priorEntry))
        {
            priorEntry.SetExpired(EvictionReason.Replaced);
        }

        if (entry.CheckExpired(utcNow))
        {
            entry.InvokeEvictionCallbacks();
            if (priorEntry != null)
            {
                coherentState.RemoveEntry(priorEntry, _options);
            }
        }
        else if (!UpdateCacheSizeExceedsCapacity(entry, priorEntry, coherentState))
        {
            bool entryAdded;
            if (priorEntry == null)
            {
                // Try to add the new entry if no previous entries exist.
                entryAdded = coherentState._entries.TryAdd(entry.Key, entry);
            }
            else
            {
                // Try to update with the new entry if a previous entries exist.
                entryAdded = coherentState._entries.TryUpdate(entry.Key, entry, priorEntry);

                if (!entryAdded)
                {
                    // The update will fail if the previous entry was removed after retrieval.
                    // Adding the new entry will succeed only if no entry has been added since.
                    // This guarantees removing an old entry does not prevent adding a new entry.
                    entryAdded = coherentState._entries.TryAdd(entry.Key, entry);
                }
            }

            if (entryAdded)
            {
                entry.AttachTokens();
            }
            else
            {
                if (_options.HasSizeLimit)
                {
                    // Entry could not be added, reset cache size
                    Interlocked.Add(ref coherentState._cacheSize, -entry.Size + (priorEntry?.Size).GetValueOrDefault());
                }
                entry.SetExpired(EvictionReason.Replaced);
                entry.InvokeEvictionCallbacks();
            }

            priorEntry?.InvokeEvictionCallbacks();
        }
        else
        {
            entry.SetExpired(EvictionReason.Capacity);
            TriggerOvercapacityCompaction();
            entry.InvokeEvictionCallbacks();
            if (priorEntry != null)
            {
                coherentState.RemoveEntry(priorEntry, _options);
            }
        }

        StartScanForExpiredItemsIfNeeded(utcNow);
    }

    /// <inheritdoc />
    public bool TryGetValue(object key, out object? result)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        CheckDisposed();

        DateTime utcNow = UtcNow;

        CoherentState coherentState = _coherentState; // Clear() can update the reference in the meantime
        if (coherentState._entries.TryGetValue(key, out CacheEntry? tmp))
        {
            CacheEntry entry = tmp;
            // Check if expired due to expiration tokens, timers, etc. and if so, remove it.
            // Allow a stale Replaced value to be returned due to concurrent calls to SetExpired during SetEntry.
            if (!entry.CheckExpired(utcNow) || entry.EvictionReason == EvictionReason.Replaced)
            {
                entry.LastAccessed = utcNow;
                result = entry.Value;

                if (TrackLinkedCacheEntries)
                {
                    // When this entry is retrieved in the scope of creating another entry,
                    // that entry needs a copy of these expiration tokens.
                    entry.PropagateOptionsToCurrent();
                }

                StartScanForExpiredItemsIfNeeded(utcNow);
                // Hit
                if (_allStats is not null)
                {
                    if (IntPtr.Size == 4)
                        Interlocked.Increment(ref GetStats().Hits);
                    else
                        GetStats().Hits++;
                }

                return true;
            }
            else
            {
                // TODO: For efficiency queue this up for batch removal
                coherentState.RemoveEntry(entry, _options);
            }
        }

        StartScanForExpiredItemsIfNeeded(utcNow);

        result = null;
        // Miss
        if (_allStats is not null)
        {
            if (IntPtr.Size == 4)
                Interlocked.Increment(ref GetStats().Misses);
            else
                GetStats().Misses++;
        }

        return false;
    }

    /// <inheritdoc />
    public void Remove(object key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        CheckDisposed();

        CoherentState coherentState = _coherentState; // Clear() can update the reference in the meantime
        if (coherentState._entries.TryRemove(key, out CacheEntry? entry))
        {
            if (_options.HasSizeLimit)
            {
                Interlocked.Add(ref coherentState._cacheSize, -entry.Size);
            }

            entry.SetExpired(EvictionReason.Removed);
            entry.InvokeEvictionCallbacks();
        }

        StartScanForExpiredItemsIfNeeded(UtcNow);
    }

    /// <summary>
    /// Removes all keys and values from the cache.
    /// </summary>
    public void Clear()
    {
        CheckDisposed();

        CoherentState oldState = Interlocked.Exchange(ref _coherentState, new CoherentState());
        foreach (KeyValuePair<object, CacheEntry> entry in oldState._entries)
        {
            entry.Value.SetExpired(EvictionReason.Removed);
            entry.Value.InvokeEvictionCallbacks();
        }
    }

    /// <summary>
    /// Gets a snapshot of the current statistics for the memory cache.
    /// </summary>
    /// <returns>Returns <see langword="null"/> if statistics are not being tracked because <see cref="MemoryCacheOptions.TrackStatistics" /> is <see langword="false"/>.</returns>
    public MemoryCacheStatistics? GetCurrentStatistics()
    {
        if (_allStats is not null)
        {
            (long hit, long miss) sumTotal = Sum();
            return new MemoryCacheStatistics()
            {
                TotalMisses = sumTotal.miss,
                TotalHits = sumTotal.hit,
                CurrentEntryCount = Count,
                CurrentEstimatedSize = _options.SizeLimit.HasValue ? Size : null
            };
        }

        return null;
    }

    internal void EntryExpired(CacheEntry entry)
    {
        _expirationQueue.Enqueue(entry);
        ProcessBatchIfNeeded();
    }

    private void ProcessBatchIfNeeded()
    {
        if (_processingExpiredItems || _expirationQueue.Count < _batchSize)
            return;

        _processingExpiredItems = true;
        Task.Run(ProcessExpiredBatch);
    }

    // Called by multiple actions to see how long it's been since we last checked for expired items.
    // If sufficient time has elapsed then a scan is initiated on a background task.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartScanForExpiredItemsIfNeeded(DateTime utcNow)
    {
        if (_options.ExpirationScanFrequency < utcNow - _lastExpirationScan)
        {
            ScheduleTask(utcNow);
        }

        void ScheduleTask(DateTime utcNow)
        {
            _lastExpirationScan = utcNow;
            Task.Factory.StartNew(state => ((MemoryCache)state!).ScanForExpiredItems(), this,
                CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }
    }

    private (long, long) Sum()
    {
        lock (_allStats!)
        {
            long hits = _accumulatedStats!.Hits;
            long misses = _accumulatedStats.Misses;

            foreach (WeakReference<Stats> wr in _allStats)
            {
                if (wr.TryGetTarget(out Stats? stats))
                {
                    hits += Interlocked.Read(ref stats.Hits);
                    misses += Interlocked.Read(ref stats.Misses);
                }
            }

            return (hits, misses);
        }
    }

    private Stats GetStats() => _stats!.Value!;

    internal sealed class Stats
    {
        private readonly MemoryCache? _memoryCache;
        public long Hits;
        public long Misses;

        public Stats() { }

        public Stats(MemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _memoryCache.AddToStats(this);
        }

        ~Stats() => _memoryCache?.RemoveFromStats(this);
    }

    private void RemoveFromStats(Stats current)
    {
        lock (_allStats!)
        {
            for (int i = 0; i < _allStats.Count; i++)
            {
                if (!_allStats[i].TryGetTarget(out Stats? stats))
                {
                    _allStats.RemoveAt(i);
                    i--;
                }
            }

            _accumulatedStats!.Hits += Interlocked.Read(ref current.Hits);
            _accumulatedStats.Misses += Interlocked.Read(ref current.Misses);
            _allStats.TrimExcess();
        }
    }

    private void AddToStats(Stats current)
    {
        lock (_allStats!)
        {
            _allStats.Add(new WeakReference<Stats>(current));
        }
    }

    private void ScanForExpiredItems()
    {
        DateTime utcNow = _lastExpirationScan = UtcNow;

        CoherentState coherentState = _coherentState; // Clear() can update the reference in the meantime
        foreach (KeyValuePair<object, CacheEntry> item in coherentState._entries)
        {
            CacheEntry entry = item.Value;

            if (entry.CheckExpired(utcNow))
            {
                coherentState.RemoveEntry(entry, _options);
            }
        }
    }

    /// <summary>
    /// Determines if increasing the cache size by the size of the
    /// entry would cause it to exceed any size limit on the cache.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> if increasing the cache size would
    /// cause it to exceed the size limit; otherwise, <see langword="false" />.
    /// </returns>
    private bool UpdateCacheSizeExceedsCapacity(CacheEntry entry, CacheEntry? priorEntry, CoherentState coherentState)
    {
        long sizeLimit = _options.SizeLimitValue;
        if (sizeLimit < 0)
        {
            return false;
        }

        long sizeRead = coherentState.Size;
        for (int i = 0; i < 100; i++)
        {
            long newSize = sizeRead + entry.Size;
            if (priorEntry != null)
            {
                Debug.Assert(entry.Key == priorEntry.Key);
                newSize -= priorEntry.Size;
            }

            if ((ulong)newSize > (ulong)sizeLimit)
            {
                // Overflow occurred, return true without updating the cache size
                return true;
            }

            long original = Interlocked.CompareExchange(ref coherentState._cacheSize, newSize, sizeRead);
            if (sizeRead == original)
            {
                return false;
            }
            sizeRead = original;
        }

        return true;
    }

    private async Task TriggerOvercapacityCompaction()
    {
        if (!await _compactionLock.WaitAsync(0))
            return;

        try
        {
            await Task.Run(() => OvercapacityCompaction());
        }
        finally
        {
            _compactionLock.Release();
        }
    }
    private async Task ProcessExpiredBatch()
    {
        try
        {
            await Task.Delay(_batchProcessDelay); // Permitir acumular más entradas

            List<CacheEntry> batch = new(_batchSize);
            while (batch.Count < _batchSize && _expirationQueue.TryDequeue(out CacheEntry? entry))
            {
                batch.Add(entry);
            }

            if (batch.Count > 0)
            {
                CoherentState coherentState = _coherentState;
                foreach (CacheEntry entry in batch)
                {
                    coherentState.RemoveEntry(entry, _options);
                }
            }
        }
        finally
        {
            _processingExpiredItems = false;
            if (_expirationQueue.Count >= _batchSize)
            {
                ProcessBatchIfNeeded();
            }
        }
    }

    private void OvercapacityCompaction()
    {
        CoherentState coherentState = _coherentState; // Clear() can update the reference in the meantime
        long currentSize = coherentState.Size;

        long sizeLimit = _options.SizeLimitValue;
        if (sizeLimit >= 0)
        {
            long lowWatermark = sizeLimit - (long)(sizeLimit * _options.CompactionPercentage);
            if (currentSize > lowWatermark)
            {
                Compact(currentSize - (long)lowWatermark, entry => entry.Size, coherentState);
            }
        }
    }

    /// Remove at least the given percentage (0.10 for 10%) of the total entries (or estimated memory?), according to the following policy:
    /// 1. Remove all expired items.
    /// 2. Bucket by CacheItemPriority.
    /// 3. Least recently used objects.
    /// ?. Items with the soonest absolute expiration.
    /// ?. Items with the soonest sliding expiration.
    /// ?. Larger objects - estimated by object graph size, inaccurate.
    public void Compact(double percentage)
    {
        CoherentState coherentState = _coherentState; // Clear() can update the reference in the meantime
        int removalCountTarget = (int)(coherentState.Count * percentage);
        Compact(removalCountTarget, _ => 1, coherentState);
    }

    private void Compact(long removalSizeTarget, Func<CacheEntry, long> computeEntrySize, CoherentState coherentState)
    {
        List<CacheEntry> entriesToRemove = [];
        // cache LastAccessed outside of the CacheEntry so it is stable during compaction
        List<(CacheEntry entry, DateTimeOffset lastAccessed)> lowPriEntries = [];
        List<(CacheEntry entry, DateTimeOffset lastAccessed)> normalPriEntries = [];
        List<(CacheEntry entry, DateTimeOffset lastAccessed)> highPriEntries = [];
        long removedSize = 0;

        // Sort items by expired & priority status
        DateTime utcNow = UtcNow;
        foreach (KeyValuePair<object, CacheEntry> item in coherentState._entries)
        {
            CacheEntry entry = item.Value;
            if (entry.CheckExpired(utcNow))
            {
                entriesToRemove.Add(entry);
                removedSize += computeEntrySize(entry);
            }
            else
            {
                switch (entry.Priority)
                {
                    case CacheItemPriority.Low:
                        lowPriEntries.Add((entry, entry.LastAccessed));
                        break;
                    case CacheItemPriority.Normal:
                        normalPriEntries.Add((entry, entry.LastAccessed));
                        break;
                    case CacheItemPriority.High:
                        highPriEntries.Add((entry, entry.LastAccessed));
                        break;
                    case CacheItemPriority.NeverRemove:
                        break;
                    default:
                        throw new NotSupportedException("Not implemented: " + entry.Priority);
                }
            }
        }

        ExpirePriorityBucket(ref removedSize, removalSizeTarget, computeEntrySize, entriesToRemove, lowPriEntries);
        ExpirePriorityBucket(ref removedSize, removalSizeTarget, computeEntrySize, entriesToRemove, normalPriEntries);
        ExpirePriorityBucket(ref removedSize, removalSizeTarget, computeEntrySize, entriesToRemove, highPriEntries);

        foreach (CacheEntry entry in entriesToRemove)
        {
            coherentState.RemoveEntry(entry, _options);
        }

        // Policy:
        // 1. Least recently used objects.
        // ?. Items with the soonest absolute expiration.
        // ?. Items with the soonest sliding expiration.
        // ?. Larger objects - estimated by object graph size, inaccurate.
        static void ExpirePriorityBucket(ref long removedSize, long removalSizeTarget, Func<CacheEntry, long> computeEntrySize, List<CacheEntry> entriesToRemove, List<(CacheEntry Entry, DateTimeOffset LastAccessed)> priorityEntries)
        {
            // Do we meet our quota by just removing expired entries?
            if (removalSizeTarget <= removedSize)
            {
                // No-op, we've met quota
                return;
            }

            // Expire enough entries to reach our goal
            // TODO: Refine policy

            // LRU
            priorityEntries.Sort(static (e1, e2) => e1.LastAccessed.CompareTo(e2.LastAccessed));
            foreach ((CacheEntry entry, _) in priorityEntries)
            {
                entry.SetExpired(EvictionReason.Capacity);
                entriesToRemove.Add(entry);
                removedSize += computeEntrySize(entry);

                if (removalSizeTarget <= removedSize)
                {
                    break;
                }
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() => Dispose(true);

    /// <summary>
    /// Disposes the cache and clears all entries.
    /// </summary>
    /// <param name="disposing"><see langword="true" /> to dispose the object resources; <see langword="false" /> to take no action.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _stats?.Dispose();
                GC.SuppressFinalize(this);
            }

            _disposed = true;
        }
    }

    private void CheckDisposed()
    {
        if (_disposed)
        {
            Throw();
        }

        [DoesNotReturn]
        static void Throw()
        {
            throw new ObjectDisposedException(typeof(MemoryCache).FullName);
        }
    }

    private static void ValidateCacheKey(object key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

    }

    /// <summary>
    /// Wrapper for the memory cache entries collection.
    ///
    /// Entries may have various sizes. If a size limit has been set, the cache keeps track of the aggregate of all the entries' sizes
    /// in order to trigger compaction when the size limit is exceeded.
    ///
    /// For performance reasons, the size is not updated atomically with the collection, but is only made eventually consistent.
    ///
    /// When the memory cache is cleared, it replaces the backing collection entirely. This may occur in parallel with operations
    /// like add, set, remove, and compact which may modify the collection and thus its overall size.
    ///
    /// To keep the overall size eventually consistent, therefore, the collection and the overall size are wrapped in this CoherentState
    /// object. Individual operations take a local reference to this wrapper object while they work, and make size updates to this object.
    /// Clearing the cache simply replaces the object, so that any still in progress updates do not affect the overall size value for
    /// the new backing collection.
    /// </summary>
    private sealed class CoherentState
    {
        internal ConcurrentDictionary<object, CacheEntry> _entries = new();
        internal long _cacheSize;

        private ICollection<KeyValuePair<object, CacheEntry>> EntriesCollection => _entries;

        internal int Count => _entries.Count;

        internal long Size => Volatile.Read(ref _cacheSize);

        internal void RemoveEntry(CacheEntry entry, MemoryCacheOptions options)
        {
            if (EntriesCollection.Remove(new KeyValuePair<object, CacheEntry>(entry.Key, entry)))
            {
                if (options.SizeLimit.HasValue)
                {
                    Interlocked.Add(ref _cacheSize, -entry.Size);
                }
                entry.InvokeEvictionCallbacks();
            }
        }
    }
}
