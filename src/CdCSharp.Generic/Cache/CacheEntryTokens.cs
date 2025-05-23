﻿using System.Diagnostics;

namespace CdCSharp.Generic.Cache;
internal sealed partial class CacheEntry
{
    // this type exists just to reduce average CacheEntry size
    // which typically is not using expiration tokens or callbacks
    private sealed class CacheEntryTokens
    {
        private List<IChangeToken>? _expirationTokens;
        private List<IDisposable>? _expirationTokenRegistrations;
        private List<PostEvictionCallbackRegistration>? _postEvictionCallbacks; // this is not really related to tokens, but was moved here to shrink typical CacheEntry size

        internal List<IChangeToken> ExpirationTokens => _expirationTokens ??= [];
        internal List<PostEvictionCallbackRegistration> PostEvictionCallbacks => _postEvictionCallbacks ??= [];

        internal void AttachTokens(CacheEntry cacheEntry)
        {
            List<IChangeToken>? expirationTokens = _expirationTokens;
            if (expirationTokens is not null)
            {
                lock (this)
                {
                    for (int i = 0; i < expirationTokens.Count; i++)
                    {
                        IChangeToken expirationToken = expirationTokens[i];
                        if (expirationToken.ActiveChangeCallbacks)
                        {
                            _expirationTokenRegistrations ??= new List<IDisposable>(1);
                            IDisposable registration = expirationToken.RegisterChangeCallback((Action<object?>)ExpirationCallback, cacheEntry);
                            _expirationTokenRegistrations.Add(registration);
                        }
                    }
                }
            }
        }

        internal bool CheckForExpiredTokens(CacheEntry cacheEntry)
        {
            List<IChangeToken>? expirationTokens = _expirationTokens;
            if (expirationTokens is not null)
            {
                for (int i = 0; i < expirationTokens.Count; i++)
                {
                    IChangeToken expiredToken = expirationTokens[i];
                    if (expiredToken.HasChanged)
                    {
                        cacheEntry.SetExpired(EvictionReason.TokenExpired);
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool CanPropagateTokens() => _expirationTokens != null;

        internal void PropagateTokens(CacheEntry parentEntry)
        {
            if (_expirationTokens != null)
            {
                lock (this)
                {
                    CacheEntryTokens parentTokens = parentEntry.GetOrCreateTokens();
                    lock (parentTokens)
                    {
                        parentTokens.ExpirationTokens.AddRange(_expirationTokens);
                    }
                }
            }
        }

        internal void DetachTokens()
        {
            // _expirationTokenRegistrations is not checked for null, because AttachTokens might initialize it under lock
            // instead we are checking for _expirationTokens, because if they are not null, then _expirationTokenRegistrations might also be not null
            if (_expirationTokens != null)
            {
                lock (this)
                {
                    List<IDisposable>? registrations = _expirationTokenRegistrations;
                    if (registrations != null)
                    {
                        _expirationTokenRegistrations = null;
                        for (int i = 0; i < registrations.Count; i++)
                        {
                            IDisposable registration = registrations[i];
                            registration.Dispose();
                        }
                    }
                }
            }
        }

        internal void InvokeEvictionCallbacks(CacheEntry cacheEntry)
        {
            if (_postEvictionCallbacks != null)
            {
                Task.Factory.StartNew(state => InvokeCallbacks((CacheEntry)state!), cacheEntry,
                    CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }
        }

        private static void InvokeCallbacks(CacheEntry entry)
        {
            Debug.Assert(entry._tokens != null);
            List<PostEvictionCallbackRegistration>? callbackRegistrations = Interlocked.Exchange(ref entry._tokens._postEvictionCallbacks, null);

            if (callbackRegistrations == null)
            {
                return;
            }

            for (int i = 0; i < callbackRegistrations.Count; i++)
            {
                PostEvictionCallbackRegistration registration = callbackRegistrations[i];

                try
                {
                    registration.EvictionCallback?.Invoke(entry.Key, entry.Value, entry.EvictionReason, registration.State);
                }
                catch (Exception e)
                {
                    // This will be invoked on a background thread, don't let it throw.
                }
            }
        }
    }
}
