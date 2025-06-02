using System;
using System.Security.Claims;

namespace Dvelop.Sdk.IdentityProvider.Client
{
    public class IdentityProviderSessionItem
    {
        public ClaimsPrincipal Principal { get; set; }

        public string Cookie { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Expire { get; set; }
    }
}