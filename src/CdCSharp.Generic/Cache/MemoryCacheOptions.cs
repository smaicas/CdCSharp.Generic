using System.ComponentModel;

namespace CdCSharp.Generic.Cache;
/// <summary>
/// Specifies options for <see cref="MemoryCache"/>.
/// </summary>
public class MemoryCacheOptions
{
    private double _compactionPercentage = 0.05;

    private const int NotSet = -1;

    /// <summary>
    /// Gets or sets the clock used by the cache for expiration.
    /// </summary>
    public ISystemClock? Clock { get; set; }

    /// <summary>
    /// Gets or sets the minimum length of time between successive scans for expired items.
    /// </summary>
    public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1);

    internal bool HasSizeLimit => SizeLimitValue >= 0;

    internal long SizeLimitValue { get; private set; } = NotSet;

    /// <summary>
    /// Gets or sets the maximum size of the cache.
    /// </summary>
    /// <remarks>
    /// The units are arbitrary. Users specify the size of every entry they add to the cache.
    /// If no size is specified, the entry has no size and the size limit is ignored for that entry.
    /// For more information, see
    /// <see href="https://learn.microsoft.com/aspnet/core/performance/caching/memory#use-setsize-size-and-sizelimit-to-limit-cache-size">Use SetSize, Size, and SizeLimit to limit cache size</see>.
    /// </remarks>
    public long? SizeLimit
    {
        get => SizeLimitValue < 0 ? null : SizeLimitValue;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} must be non-negative.");
            }

            SizeLimitValue = value ?? NotSet;
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the cache is compacted when the maximum size is exceeded.
    /// </summary>
    [EditorBrowsableAttribute(EditorBrowsableState.Never)]
    [Obsolete("This property is retained only for compatibility.  Remove use and instead call MemoryCache.Compact as needed.", error: true)]
    public bool CompactOnMemoryPressure { get; set; }

    /// <summary>
    /// Gets or sets the amount the cache is compacted by when the maximum size is exceeded.
    /// </summary>
    public double CompactionPercentage
    {
        get => _compactionPercentage;
        set
        {
            if (value is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} must be between 0 and 1 inclusive.");
            }

            _compactionPercentage = value;
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether linked entries are tracked.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if linked entries are tracked; otherwise, <see langword="false" />.
    /// The default is <see langword="false" />.
    /// </value>
    /// <remarks>Prior to .NET 7, this feature was always enabled.</remarks>
    public bool TrackLinkedCacheEntries { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether memory cache statistics are tracked.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if memory cache statistics are tracked; otherwise, <see langword="false" />.
    /// The default is <see langword="false" />.
    /// </value>
    public bool TrackStatistics { get; set; }
}
