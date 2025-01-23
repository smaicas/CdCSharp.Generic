using CdCSharp.Generic.Cache;

namespace CdCSharp.Generic.UnitTests.Cache;
public class CacheEntry_Tests
{
    [Fact]
    public void Constructor_ValidatesArguments()
    {
        MemoryCache cache = new();
        Assert.Throws<ArgumentNullException>(() => new CacheEntry(null!, cache));
        Assert.Throws<ArgumentNullException>(() => new CacheEntry("key", null!));
    }

    [Fact]
    public void Size_ValidatesValue()
    {
        MemoryCache cache = new();
        ICacheEntry entry = cache.CreateEntry("key");
        Assert.Throws<ArgumentOutOfRangeException>(() => ((ICacheEntry)entry).Size = -1);
    }

    [Fact]
    public void AbsoluteExpiration_SetsAndGetsCorrectly()
    {
        MemoryCache cache = new();
        ICacheEntry entry = cache.CreateEntry("key");
        DateTimeOffset expiration = DateTimeOffset.UtcNow.AddHours(1);

        ((ICacheEntry)entry).AbsoluteExpiration = expiration;
        Assert.Equal(expiration, ((ICacheEntry)entry).AbsoluteExpiration);
    }

    [Fact]
    public void AbsoluteExpirationRelativeToNow_ValidatesValue()
    {
        MemoryCache cache = new();
        ICacheEntry entry = cache.CreateEntry("key");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ((ICacheEntry)entry).AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(-1));
    }

    [Fact]
    public void SlidingExpiration_ValidatesValue()
    {
        MemoryCache cache = new();
        ICacheEntry entry = cache.CreateEntry("key");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            entry.SlidingExpiration = TimeSpan.FromSeconds(-1));
    }
}
