using System;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace Dvelop.Sdk.IdentityProvider.Client
{
    public class IdentityProviderSessionStore
    {
        private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        public ClaimsPrincipal GetPrincipal(string cookie)
        {
            if (!_memoryCache.TryGetValue(cookie, out IdentityProviderSessionItem sessionItem)) return null;
            return sessionItem.Expire > DateTimeOffset.Now ? sessionItem.Principal : null;
        }

        public void SetPrincipal(string cookie, DateTimeOffset expire, ClaimsPrincipal principal)
        {
            var sessionItem = new IdentityProviderSessionItem
            {
                Cookie = cookie,
                Principal = principal,
                Expire = expire
            };
            _memoryCache.Set(cookie, sessionItem, new MemoryCacheEntryOptions { AbsoluteExpiration =  expire});
        }

    }
}