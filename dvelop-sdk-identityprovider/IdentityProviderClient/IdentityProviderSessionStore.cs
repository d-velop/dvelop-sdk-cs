using System;
using System.Collections.Concurrent;

using System.Security.Claims;
using Microsoft.Extensions.Internal;

namespace Dvelop.Sdk.IdentityProvider.Client
{
    public class IdentityProviderSessionStore
    {
        private readonly ISystemClock _clock;
        private readonly int _cleanupThreshold;

        private readonly ConcurrentDictionary<string, IdentityProviderSessionItem> _sessionCache =
            new ConcurrentDictionary<string, IdentityProviderSessionItem>();

        private int _cleanupCounter;
        private readonly object _cleanupLock=new object();

        public IdentityProviderSessionStore(ISystemClock clock, int cleanupThreshold)
        {
            _clock = clock;
            _cleanupThreshold = cleanupThreshold;
        }

        public IdentityProviderSessionStore():this(new SystemClock(), 20)
        {
            
        }
        

        public ClaimsPrincipal GetPrincipal(string cookie)
        {
            CleanUp();
            var id = IdFromCookie(cookie);
            if (!_sessionCache.TryGetValue(id, out var sessionItem)) return null;
            if (sessionItem.Expire.CompareTo(_clock.UtcNow) < 0)
            {
                _sessionCache.TryRemove(id,out _);
                return null;
            }
            return sessionItem.Cookie.Equals(cookie) ? sessionItem.Principal : null;
        }

        public void SetPrincipal(string cookie, DateTimeOffset expire, ClaimsPrincipal principal)
        {
            CleanUp();
            var id = IdFromCookie(cookie);
            var sessionItem = new IdentityProviderSessionItem
            {
                Cookie = cookie,
                Principal = principal,
                Expire = expire
            };
            _sessionCache[id] = sessionItem;
        }

        private void CleanUp()
        {
            _cleanupCounter++;
            if (_cleanupCounter < _cleanupThreshold) return;
            lock (_cleanupLock)
            {
                if (_cleanupCounter < _cleanupThreshold) return;
                _cleanupCounter = 0;
                try
                {
                    foreach (var key in _sessionCache.Keys)
                    {
                        if (!_sessionCache.TryGetValue(key, out var sessionItem)) continue;
                        if (sessionItem.Expire.CompareTo(_clock.UtcNow) < 0)
                        {
                            _sessionCache.TryRemove(key, out _);
                        }
                    }
                }
                catch (Exception)
                {
                    //ignorieren
                }
            }
        }

        private static string IdFromCookie(string cookie)
        {
            return cookie;
        }
    }
}