using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;

namespace Dvelop.Sdk.IdentityProvider.Client
{
    public class IdentityProviderSessionStore
    {
        #region Fields

        private readonly ConcurrentDictionary<string, IdentityProviderSessionItem> _sessionCache =
            new ConcurrentDictionary<string, IdentityProviderSessionItem>();

        private int _cleanupCounter = 0;
        private readonly object _cleanupLock=new object();
        #endregion

        #region Public Methods

        public ClaimsPrincipal GetPrincipal(string cookie)
        {
            CleanUp();
            var id = IdFromCookie(cookie);
            if (!_sessionCache.TryGetValue(id, out IdentityProviderSessionItem sessionItem)) return null;
            if (sessionItem.Expire.CompareTo(DateTime.Now) < 0)
            {
                _sessionCache.TryRemove(id,out _);
                return null;
            }
            return sessionItem.Cookie.Equals(cookie) ? sessionItem.Principal : null;
        }

        public void SetPrincipal(string cookie, DateTime expire, ClaimsPrincipal principal)
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

        #endregion

        #region Private Methods

        private void CleanUp()
        {
            _cleanupCounter++;
            if (_cleanupCounter < 20) return;
            lock (_cleanupLock)
            {
                if (_cleanupCounter < 20) return;
                _cleanupCounter = 0;
                try
                {
                    foreach (var key in _sessionCache.Keys)
                    {
                        if (_sessionCache.TryGetValue(key, out IdentityProviderSessionItem sessionItem))
                        {
                            if (sessionItem.Expire.CompareTo(DateTime.Now) < 0)
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
            var elements = cookie.Split('&');
            return elements.Any() ? elements[0] : null;
        }

        #endregion
    }
}