namespace CdCSharp.Generic.Cache;
/// <summary>
/// Represents a local in-memory cache whose values are not serialized.
/// </summary>
public interface IMemoryCache : IDisposable
{
    /// <summary>
    /// Gets the item associated with this key if present.
    /// </summary>
    /// <param name="key">An object identifying the requested entry.</param>
    /// <param name="value">The located value or null.</param>
    /// <returns>True if the key was found.</returns>
    bool TryGetValue(object key, out object? value);

    /// <summary>
    /// Create or overwrite an entry in the cache.
    /// </summary>
    /// <param name="key">An object identifying the entry.</param>
    /// <returns>The newly created <see cref="ICacheEntry"/> instance.</returns>
    ICacheEntry CreateEntry(object key);

    /// <summary>
    /// Removes the object associated with the given key.
    /// </summary>
    /// <param name="key">An object identifying the entry.</param>
    void Remove(object key);

#if NET
    /// <summary>
    /// Gets a snapshot of the cache statistics if available.
    /// </summary>
    /// <returns>An instance of <see cref="MemoryCacheStatistics"/> containing a snapshot of the cache statistics.</returns>
    MemoryCacheStatistics? GetCurrentStatistics() => null;
#endif
}
