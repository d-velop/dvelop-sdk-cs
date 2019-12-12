﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dvelop.Sdk.IdentityProvider.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Dvelop.Sdk.IdentityProvider.Middleware
{
    public class IdentityProviderMiddleware 
    {
        private readonly IdentityProviderClient _identityProviderClient;
        private readonly RequestDelegate _next;
        

        public IdentityProviderMiddleware(RequestDelegate next, IdentityProviderOptions options, HttpClient client)
        {
            _next = next;
            _identityProviderClient = new IdentityProviderClient(client, options.BaseAddress, options.TenantInformationCallback, options.AllowExternalValidation);
        }
        
        private bool RedirectToLogin(HttpContext context)
        {
            if (!string.IsNullOrWhiteSpace(context?.User?.Identity?.Name))
            {
                return false;
            }

            var accept = context?.Request.Headers["accept"];
            
            var mediaTypeWithQualityHeaderValue = GetMediaTypes(accept)?.FirstOrDefault();
            if (mediaTypeWithQualityHeaderValue != null)
            {
                if (mediaTypeWithQualityHeaderValue.MediaType != "text/html" &&
                    mediaTypeWithQualityHeaderValue.MediaType != "text/*" &&
                    mediaTypeWithQualityHeaderValue.MediaType != "*/*" &&
                    mediaTypeWithQualityHeaderValue.MediaType != "")
                {
                    context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                    context.Response.Headers.Add("WWW-Authenticate", "Bearer");
                    return true;
                }
            }

            if (context.Request.Method != "GET" && context.Request.Method != "HEAD" )
            {
                context.Response.Headers.Add("WWW-Authenticate","Bearer");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return true;
            }
            

            var querystring=string.Empty;
            foreach (var query in context.Request.Query)
            {
                if (!string.IsNullOrWhiteSpace(querystring))
                    querystring += "&";
                querystring += query.Key + "=" + query.Value;
            }
            string logonuri = context.Request.Path;
            if (!string.IsNullOrWhiteSpace(querystring))
            {
                logonuri += "?" + querystring;
            }
            context.Response.Redirect(
                _identityProviderClient.GetLoginUri(logonuri).ToString());
            return true;
        }

        public async Task Invoke(HttpContext context)
        {
            var sessionId = context.GetAuthSessionIdFromCookie();
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                sessionId = context.GetAuthSessionIdFromHeader();
                
            }
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                context.User = await _identityProviderClient.GetClaimsPrincipalAsync(sessionId);
            }
            
            if (RedirectToLogin( context ))
                return;

            await _next.Invoke(context);
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