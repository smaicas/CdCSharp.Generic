﻿namespace CdCSharp.Generic.Cache;
/// <summary>
/// Provides access to the normal system clock.
/// </summary>
public class SystemClock : ISystemClock
{
    /// <summary>
    /// Retrieves the current system time in UTC.
    /// </summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
