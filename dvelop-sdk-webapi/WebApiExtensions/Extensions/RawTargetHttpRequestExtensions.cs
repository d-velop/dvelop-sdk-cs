using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Dvelop.Sdk.WebApiExtensions.Extensions
{
    public static class RawTargetHttpRequestExtensions
    {
        public static string GetRawUrl(this HttpRequest request)
        {
            var httpContext = request.HttpContext;
            var requestFeature = httpContext.Features.Get<IHttpRequestFeature>();
            return requestFeature?.RawTarget;
        }
    }
}