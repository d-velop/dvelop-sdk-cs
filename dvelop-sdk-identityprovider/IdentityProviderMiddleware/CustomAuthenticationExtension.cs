using System;
using Microsoft.AspNetCore.Authentication;

namespace Dvelop.Sdk.IdentityProvider.Middleware
{
    public static class CustomAuthenticationExtensions
    {
        public static AuthenticationBuilder AddIdentityProviderAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<CustomAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<CustomAuthenticationOptions, CustomAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }

}
