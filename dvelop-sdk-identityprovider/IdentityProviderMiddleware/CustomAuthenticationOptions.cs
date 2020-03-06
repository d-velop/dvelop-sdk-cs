using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Dvelop.Sdk.IdentityProvider.Middleware
{
    public class CustomAuthenticationOptions : AuthenticationSchemeOptions
    {
        public ClaimsIdentity Identity { get; set; }
    }
}