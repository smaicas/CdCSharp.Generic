﻿namespace CdCSharp.Generic.Cache;
/// <summary>
/// Propagates notifications that a change has occurred.
/// </summary>
public interface IChangeToken
{
    /// <summary>
    /// Gets a value that indicates if a change has occurred.
    /// </summary>
    bool HasChanged { get; }

    /// <summary>
    /// Indicates if this token will proactively raise callbacks. If <c>false</c>, the token consumer must
    /// poll <see cref="HasChanged" /> to detect changes.
    /// </summary>
    bool ActiveChangeCallbacks { get; }

    /// <summary>
    /// Registers for a callback that will be invoked when the entry has changed.
    /// <see cref="HasChanged"/> MUST be set before the callback is invoked.
    /// </summary>
    /// <param name="callback">The <see cref="Action{Object}"/> to invoke.</param>
    /// <param name="state">State to be passed into the callback.</param>
    /// <returns>An <see cref="IDisposable"/> that is used to unregister the callback.</returns>
    IDisposable RegisterChangeCallback(Action<object?> callback, object? state);
}
