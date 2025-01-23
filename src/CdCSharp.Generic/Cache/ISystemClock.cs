namespace CdCSharp.Generic.Cache;
public interface ISystemClock
{
    /// <summary>
    /// Retrieves the current system time in UTC.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
