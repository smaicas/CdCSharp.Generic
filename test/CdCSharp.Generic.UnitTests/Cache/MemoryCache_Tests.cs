using CdCSharp.Generic.Cache;
using System.Collections.Concurrent;

namespace CdCSharp.Generic.UnitTests.Cache;
public class MemoryCache_Tests
{
    [Fact]
    public void Constructor_WithDefaultOptions_CreatesInstance()
    {
        MemoryCache cache = new();
        Assert.NotNull(cache);
    }

    [Fact]
    public void Constructor_WithCustomOptions_SetsProperties()
    {
        MemoryCache cache = new(options =>
        {
            options.SizeLimit = 100;
            options.CompactionPercentage = 0.1;
            options.TrackStatistics = true;
        });

        MemoryCacheStatistics? stats = cache.GetCurrentStatistics();
        Assert.NotNull(stats);
    }

    [Fact]
    public void CreateEntry_WithValidKey_ReturnsNewEntry()
    {
        MemoryCache cache = new();
        ICacheEntry entry = cache.CreateEntry("key1");
        Assert.NotNull(entry);
        Assert.Equal("key1", entry.Key);
    }

    [Fact]
    public void CreateEntry_WithNullKey_ThrowsArgumentNullException()
    {
        MemoryCache cache = new();
        Assert.Throws<ArgumentNullException>(() => cache.CreateEntry(null!));
    }

    [Fact]
    public void Set_AndGet_BasicOperation()
    {
        MemoryCache cache = new();
        using (ICacheEntry entry = cache.CreateEntry("key1"))
        {
            entry.Value = "value1";
        }

        Assert.True(cache.TryGetValue("key1", out object? value));
        Assert.Equal("value1", value);
    }

    [Fact]
    public void Remove_ExistingKey_RemovesItem()
    {
        MemoryCache cache = new();
        using (ICacheEntry entry = cache.CreateEntry("key1"))
        {
            entry.Value = "value1";
        }

        cache.Remove("key1");
        Assert.False(cache.TryGetValue("key1", out _));
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        MemoryCache cache = new();
        using (ICacheEntry entry1 = cache.CreateEntry("key1"))
        {
            entry1.Value = "value1";
        }
        using (ICacheEntry entry2 = cache.CreateEntry("key2"))
        {
            entry2.Value = "value2";
        }

        cache.Clear();
        Assert.Equal(0, cache.Count);
        Assert.False(cache.TryGetValue("key1", out _));
        Assert.False(cache.TryGetValue("key2", out _));
    }

    [Fact]
    public void Expiration_AbsoluteExpiration_ExpiredItemNotReturned()
    {
        MemoryCache cache = new();
        DateTimeOffset expirationTime = DateTimeOffset.UtcNow.AddMilliseconds(50);

        using (ICacheEntry entry = cache.CreateEntry("key1"))
        {
            entry.Value = "value1";
            entry.AbsoluteExpiration = expirationTime;
        }

        Thread.Sleep(100);
        Assert.False(cache.TryGetValue("key1", out _));
    }

    [Fact]
    public void Expiration_SlidingExpiration_ExpiredItemNotReturned()
    {
        MemoryCache cache = new();
        using (ICacheEntry entry = cache.CreateEntry("key1"))
        {
            entry.Value = "value1";
            entry.SlidingExpiration = TimeSpan.FromMilliseconds(50);
        }

        Thread.Sleep(100);
        Assert.False(cache.TryGetValue("key1", out _));
    }

    [Fact]
    public void SizeLimit_ExceedsCapacity_TriggersCompaction()
    {
        MemoryCache cache = new(options =>
        {
            options.SizeLimit = 10;
            options.CompactionPercentage = 0.5;
        });

        for (int i = 0; i < 5; i++)
        {
            using ICacheEntry entry = cache.CreateEntry($"key{i}");
            entry.Size = 3;
            entry.Value = $"value{i}";
        }

        // Allow time for compaction
        Thread.Sleep(200);
        Assert.True(cache.Count < 5);
    }

    [Fact]
    public void Concurrent_MultipleThreads_HandlesRaceConditions()
    {
        MemoryCache cache = new();
        ConcurrentDictionary<string, string> concurrentDict = new();
        List<Task> tasks = [];

        for (int i = 0; i < 100; i++)
        {
            string key = $"key{i}";
            string value = $"value{i}";
            tasks.Add(Task.Run(() =>
            {
                using ICacheEntry entry = cache.CreateEntry(key);
                entry.Value = value;
                concurrentDict[key] = value;
            }));
        }

        Task.WaitAll(tasks.ToArray());

        foreach (KeyValuePair<string, string> kvp in concurrentDict)
        {
            Assert.True(cache.TryGetValue(kvp.Key, out object? value));
            Assert.Equal(kvp.Value, value);
        }
    }

    [Fact]
    public void Statistics_TrackingEnabled_RecordsHitsAndMisses()
    {
        MemoryCache cache = new(options => options.TrackStatistics = true);

        using (ICacheEntry entry = cache.CreateEntry("key1"))
        {
            entry.Value = "value1";
        }

        // Hit
        cache.TryGetValue("key1", out _);
        // Miss
        cache.TryGetValue("nonexistent", out _);

        MemoryCacheStatistics? stats = cache.GetCurrentStatistics();
        Assert.NotNull(stats);
        Assert.Equal(1, stats.TotalHits);
        Assert.Equal(1, stats.TotalMisses);
    }

    [Fact]
    public void PostEvictionCallback_Triggered_WhenItemExpires()
    {
        bool callbackInvoked = false;

        MemoryCache cache = new(options => options.ExpirationScanFrequency = TimeSpan.FromMilliseconds(50));

        using (ICacheEntry entry = cache.CreateEntry("key1"))
        {
            entry.Value = "value1";
            entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = (key, value, reason, state) => callbackInvoked = true
            });
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(100);
        }

        // Espera activa hasta que se invoque el callback o expire el tiempo máximo
        TimeSpan timeout = TimeSpan.FromSeconds(1);
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!callbackInvoked && stopwatch.Elapsed < timeout)
        {
            Thread.Sleep(10); // Pequeña espera para evitar usar mucho CPU
            cache.TryGetValue("key1", out _); // Forzar operación de cache para activar limpieza
        }

        // Validar que el callback fue llamado
        Assert.True(callbackInvoked, "El callback de post-evicción no fue invocado.");

        // Verificar que la entrada no existe en el cache
        Assert.False(cache.TryGetValue("key1", out _), "La entrada aún existe en el cache después de expirar.");
    }

    [Fact]
    public void LinkedEntries_PropagateExpirationSettings()
    {
        MemoryCache cache = new(options => options.TrackLinkedCacheEntries = true);

        using (ICacheEntry entry1 = cache.CreateEntry("key1"))
        {
            entry1.Value = "value1";
            entry1.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(50);

            using (ICacheEntry entry2 = cache.CreateEntry("key2"))
            {
                entry2.Value = "value2";
            }
        }

        Thread.Sleep(100);
        Assert.False(cache.TryGetValue("key1", out _));
        Assert.True(cache.TryGetValue("key2", out _));
    }

    [Fact]
    public void Dispose_PreventsNewOperations()
    {
        MemoryCache cache = new();
        cache.Dispose();

        Assert.Throws<ObjectDisposedException>(() => cache.CreateEntry("key1"));
        Assert.Throws<ObjectDisposedException>(() => cache.TryGetValue("key1", out _));
        Assert.Throws<ObjectDisposedException>(() => cache.Remove("key1"));
    }
}

