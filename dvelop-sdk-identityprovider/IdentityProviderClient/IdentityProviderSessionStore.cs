using System;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Dvelop.Sdk.IdentityProvider.Client
{
    public class IdentityProviderSessionStore
    {
        private TimeSpan CacheRefreshTimeout = TimeSpan.FromMinutes(5);
        private readonly TimeProvider _clock;
        private readonly int _cleanupThreshold;

        private readonly ConcurrentDictionary<string, IdentityProviderSessionItem> _sessionCache =
            new ConcurrentDictionary<string, IdentityProviderSessionItem>();

        private int _cleanupCounter;
        private readonly object _cleanupLock=new object();

        public IdentityProviderSessionStore(TimeProvider clock, int cleanupThreshold)
        {
            _clock = clock;
            _cleanupThreshold = cleanupThreshold;
        }

        public IdentityProviderSessionStore():this(TimeProvider.System, 20)
        {
            
        }
        

        public ClaimsPrincipal GetPrincipal(string cookie)
        {
            return GetPrincipal(cookie, out _);
        }

        public ClaimsPrincipal GetPrincipal(string cookie, out bool doRefresh)
        {
            //CleanUp();
            doRefresh = false;
            var id = IdFromCookie(cookie);
            var now = _clock.GetUtcNow();
            if (!_sessionCache.TryGetValue(id, out var sessionItem)) return null;
            if (sessionItem.Expire.CompareTo(now) < 0)
            {
                _sessionCache.TryRemove(id,out _);
                return null;
            }
            var result = sessionItem.Cookie.Equals(cookie) ? sessionItem.Principal : null;
            if (result != null)
            {
                doRefresh = sessionItem.Expire - now < CacheRefreshTimeout;
            }
            return result;
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
                        if (sessionItem.Expire.CompareTo(_clock.GetUtcNow()) < 0)
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