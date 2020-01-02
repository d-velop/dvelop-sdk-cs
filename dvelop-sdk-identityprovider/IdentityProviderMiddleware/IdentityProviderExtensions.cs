using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Dvelop.Sdk.IdentityProvider.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Dvelop.Sdk.IdentityProvider.Middleware
{
    public static class IdentityProviderExtensions
    {
        private const string AuthorizationHeader = "Authorization";
        public static void UseIdentityProvider(this IApplicationBuilder app, IdentityProviderOptions options = null)
        {
            app.UseMiddleware<IdentityProviderMiddleware>(options??new IdentityProviderOptions());
        }

        private static string GetAuthSessionId(this HttpContext context)
        {
            var sessionId = GetAuthSessionIdFromCookie(context);
            if (string.IsNullOrWhiteSpace(sessionId))
                sessionId = GetAuthSessionIdFromHeader(context);
            return sessionId;
        }
        internal static string GetAuthSessionIdFromCookie(this HttpContext context)
        {
            return context.Request.Cookies.FirstOrDefault(c => c.Key.Equals(IdentityProviderClient.CookieName)).Value;
        }

        internal static string GetAuthSessionIdFromHeader(this HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(AuthorizationHeader)) return null;
            var auth = context.Request.Headers[AuthorizationHeader];
            if (auth.Count == 0)  return null;
            var authValues = auth[0].Split(' ');
            if (authValues.Length < 2) return null;
            return authValues[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase) ? authValues[1] : null;
        }
    }
}