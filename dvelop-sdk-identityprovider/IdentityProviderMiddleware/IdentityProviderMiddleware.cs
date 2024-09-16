using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dvelop.Sdk.IdentityProvider.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Dvelop.Sdk.IdentityProvider.Middleware
{
    public class IdentityProviderMiddleware 
    {
        private readonly IdentityProviderClient _identityProviderClient;
        private readonly RequestDelegate _next;

        public IdentityProviderMiddleware(RequestDelegate next, IdentityProviderOptions clientOptions)
        {
            _next = next;
            var co = new IdentityProviderClientOptions
            {
                HttpClient = clientOptions.HttpClient,
                AllowAppSessions = clientOptions.AllowAppSessions,
                AllowedImpersonatedApps = clientOptions.AllowedImpersonatedApps,
                AllowExternalValidation = clientOptions.AllowExternalValidation,
                AllowImpersonatedUsers = clientOptions.AllowImpersonatedUsers,
                LogCallBack = clientOptions.LogCallBack,
                SystemBaseUri = clientOptions.BaseAddress,
                TenantInformationCallback = clientOptions.TenantInformationCallback,
                UseMinimizedOnlyIdValidateDetailLevel = clientOptions.UseMinimizedOnlyIdValidateDetailLevel
            };
            
            _identityProviderClient = new IdentityProviderClient( co );
        }
        
        public async Task Invoke(HttpContext context)
        {
            var sessionId = context.GetAuthSessionId();

            var bearerTokenReceived = string.IsNullOrWhiteSpace(sessionId);
            if (!bearerTokenReceived)
            {
                context.User = await _identityProviderClient.GetClaimsPrincipalAsync(sessionId).ConfigureAwait(false);
            }
            
            
            context.Response.OnStarting(_ => Task.FromResult(context.Response.StatusCode == (int)HttpStatusCode.Unauthorized &&
                                                             RequestRedirectedToLogin(context)), context.Response);
            
            await _next.Invoke(context).ConfigureAwait(false);
        }
        
        private bool RequestRedirectedToLogin(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!string.IsNullOrWhiteSpace(context.User.Identity?.Name))
            {
                return false;
            }
      
            if (HandleUnauthorizedRequest(context))
            {
                return true;
            }
            
            var endpoint = context.GetEndpoint();
            var anon = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>();
            
            if (anon == null)
            {
                return true;
            }
            
            var encodedUrl = context.Request.GetEncodedPathAndQuery();
            context.Response.Redirect(_identityProviderClient.GetLoginUri(encodedUrl).ToString());
            
            return true;
        }

        private static bool HandleUnauthorizedRequest(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var accept = context.Request.Headers["accept"];
            var mediaTypeWithQualityHeaderValue = GetMediaTypes(accept)?.FirstOrDefault();
            if (mediaTypeWithQualityHeaderValue != null)
            {
                if (mediaTypeWithQualityHeaderValue.MediaType != "text/html" &&
                    mediaTypeWithQualityHeaderValue.MediaType != "text/*" &&
                    mediaTypeWithQualityHeaderValue.MediaType != "*/*" &&
                    mediaTypeWithQualityHeaderValue.MediaType != "")
                {
                    context.Response.Headers["WWW-Authenticate"] = "Bearer";
                    context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                    return true;
                }
            }

            if (context.Request.Method == "GET" || context.Request.Method == "HEAD") return false;
            context.Response.Headers["WWW-Authenticate"] = "Bearer";
            context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            return true;

        }

        
        
        private static IEnumerable<MediaTypeWithQualityHeaderValue> GetMediaTypes(string headerValues)
        {
            if (string.IsNullOrEmpty(headerValues))
            {
                return new List<MediaTypeWithQualityHeaderValue>();
            }
            
            return headerValues.Split(',')
                .Select(headerValue =>
                {
                    var x = MediaTypeWithQualityHeaderValue.TryParse(headerValue, out var mediaTypeHeaderValue)
                        ? mediaTypeHeaderValue
                        : new MediaTypeWithQualityHeaderValue("application/octed-stream");
                    return x;
                })
                .Where(h => h.Quality.GetValueOrDefault(1) > 0)
                .OrderByDescending(mt => mt.Quality.GetValueOrDefault(1));
        }
    }
}