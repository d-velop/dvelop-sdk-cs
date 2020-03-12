using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dvelop.Sdk.IdentityProvider.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dvelop.Sdk.IdentityProvider.Middleware
{
    public class IdentityProviderMiddleware 
    {
        private readonly IdentityProviderClient _identityProviderClient;
        private readonly RequestDelegate _next;

        public IdentityProviderMiddleware(RequestDelegate next, IdentityProviderOptions options, ILoggerFactory factory)
        {
            _next = next;
            _identityProviderClient = new IdentityProviderClient(factory, options.HttpClient, options.TenantInformationCallback, options.AllowExternalValidation);
        }
        
        public async Task Invoke(HttpContext context)
        {
            var sessionId = context.GetAuthSessionId();

            var bearerTokenReceived = string.IsNullOrWhiteSpace(sessionId);
            if (!bearerTokenReceived)
            {
                context.User = await _identityProviderClient.GetClaimsPrincipalAsync(sessionId);
            }
            context.Response.OnStarting(state =>
            {
                if (context.Response.StatusCode != (int) HttpStatusCode.Unauthorized)
                {
                    return Task.FromResult(false);
                }
                return Task.FromResult(RequestRedirectedToLogin(context));
            }, context.Response);
            
            await _next.Invoke(context);
        }
        
        private bool RequestRedirectedToLogin(HttpContext context)
        {
            if(context == null) { throw new ArgumentNullException(nameof(context));}
            
            if (!string.IsNullOrWhiteSpace(context.User?.Identity?.Name))
            {
                return false;
            }
      
            if (HandleUnauthorizedRequest(context))
            {
                return true;
            }
            
            var querystring=string.Empty;
            foreach (var query in context.Request.Query)
            {
                if (!string.IsNullOrWhiteSpace(querystring))
                    querystring += "&";
                querystring += query.Key + "=" + query.Value;
            }
            string logonUri = context.Request.PathBase + context.Request.Path;

            if (!string.IsNullOrWhiteSpace(querystring))
            {
                logonUri += "?" + querystring;
            }
            
            var redirectLocation = _identityProviderClient.GetLoginUri(logonUri).ToString();
            context.Response.Redirect(redirectLocation);
            return true;
        }

        private static bool HandleUnauthorizedRequest(HttpContext context)
        {
            if(context== null) { throw new ArgumentNullException(nameof(context));}

            var accept = context.Request.Headers["accept"];
            var mediaTypeWithQualityHeaderValue = GetMediaTypes(accept)?.FirstOrDefault();
            if (mediaTypeWithQualityHeaderValue != null)
            {
                if (mediaTypeWithQualityHeaderValue.MediaType != "text/html" &&
                    mediaTypeWithQualityHeaderValue.MediaType != "text/*" &&
                    mediaTypeWithQualityHeaderValue.MediaType != "*/*" &&
                    mediaTypeWithQualityHeaderValue.MediaType != "")
                {
                    context.Response.Headers?.Add("WWW-Authenticate", "Bearer");
                    context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                    return true;
                }
            }

            if (context.Request.Method == "GET" || context.Request.Method == "HEAD") return false;
            context.Response.Headers?.Add("WWW-Authenticate", "Bearer");
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
                .Where(h => h?.Quality.GetValueOrDefault(1) > 0)
                .OrderByDescending(mt => mt.Quality.GetValueOrDefault(1));
        }
    }
}