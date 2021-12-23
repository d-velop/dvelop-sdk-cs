using System.Threading.Tasks;
using Dvelop.Sdk.BaseInterfaces;
using Dvelop.Sdk.Logging.Abstractions.Scope;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dvelop.Sdk.WebApiExtensions.Middleware
{
    public class TenantLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<TenantLoggingMiddleware> _logger;


        public TenantLoggingMiddleware(RequestDelegate next, ITenantContext tenantContext, ILogger<TenantLoggingMiddleware> logger)
        {
            _next = next;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (_logger.BeginScope(new TenantLogScope(_tenantContext?.TenantId)))
            {
                await _next(context);
            }
        }
    }
}