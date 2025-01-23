namespace CdCSharp.Generic.Cache;

/// <summary>
/// Represents the callback method that gets called when a cache entry expires.
/// </summary>
/// <param name="key">The key of the entry being evicted.</param>
/// <param name="value">The value of the entry being evicted.</param>
/// <param name="reason">The <see cref="EvictionReason"/>.</param>
/// <param name="state">The information that was passed when registering the callback.</param>
public delegate void PostEvictionDelegate(object key, object? value, EvictionReason reason, object? state);

/// <summary>
/// Represents a callback delegate that will be fired after an entry is evicted from the cache.
/// </summary>
public class PostEvictionCallbackRegistration
{
    /// <summary>
    /// Gets or sets the callback delegate that will be fired after an entry is evicted from the cache.
    /// </summary>
    public PostEvictionDelegate? EvictionCallback { get; set; }

    /// <summary>
    /// Gets or sets the state to pass to the callback delegate.
    /// </summary>
    public object? State { get; set; }
}

/// <summary>
/// Specifies the reasons why an entry was evicted from the cache.
/// </summary>
public enum EvictionReason
{
    /// <summary>
    /// The item was not removed from the cache.
    /// </summary>
    None,

    /// <summary>
    /// The item was removed from the cache manually.
    /// </summary>
    Removed,

    /// <summary>
    /// The item was removed from the cache because it was overwritten.
    /// </summary>
    Replaced,

    /// <summary>
    /// The item was removed from the cache because it timed out.
    /// </summary>
    Expired,

    /// <summary>
    /// The item was removed from the cache because its token expired.
    /// </summary>
    TokenExpired,

    /// <summary>
    /// The item was removed from the cache because it exceeded its capacity.
    /// </summary>
    Capacity,
}