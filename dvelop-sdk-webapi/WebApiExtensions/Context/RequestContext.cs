using System.Diagnostics;
using Dvelop.Sdk.BaseInterfaces;
using Microsoft.AspNetCore.Http;

namespace Dvelop.Sdk.WebApiExtensions.Context
{
    public class RequestContext: IRequestContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public string DvRequestId => _httpContextAccessor.HttpContext.Request.Headers["x-dv-request-id"];

        public string W3CTraceId => Activity.Current?.Id ?? _httpContextAccessor.HttpContext.TraceIdentifier;
    }
}