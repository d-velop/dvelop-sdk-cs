using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;

namespace Dvelop.Sdk.IdentityProvider.Client
{
    public class IdentityProviderSessionStore
    {
        private readonly ConcurrentDictionary<string, IdentityProviderSessionItem> _sessionCache =
            new ConcurrentDictionary<string, IdentityProviderSessionItem>();

        public ClaimsPrincipal GetPrincipal(string cookie)
        {
            var id = IdFromCookie(cookie);
            if (!_sessionCache.TryGetValue(id, out var sessionItem)) return null;
            if (sessionItem.Expire.CompareTo(DateTime.Now) < 0) return null;
            return sessionItem.Cookie.Equals(cookie) ? sessionItem.Principal : null;
        }

        public void SetPrincipal(string cookie, DateTime expire, ClaimsPrincipal principal)
        {
            var id = IdFromCookie(cookie);
            var sessionItem = new IdentityProviderSessionItem
            {
                Cookie = cookie,
                Principal = principal,
                Expire = expire
            };
            _sessionCache[id] = sessionItem;
        }

        private static string IdFromCookie(string cookie)
        {
            var elements = cookie.Split('&');
            return elements.Any() ? elements[0] : null;
        }
    }
}