using System.Diagnostics;
using System.Threading.Tasks;
using Dvelop.Sdk.BaseInterfaces;
using Dvelop.Sdk.Logging.Abstractions.Extension;
using Dvelop.Sdk.Logging.Abstractions.Scope;
using Dvelop.Sdk.Logging.Abstractions.State;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Dvelop.Sdk.WebApiExtensions.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly ILogger<RequestLoggingMiddleware> _logger;
 
        private readonly RequestDelegate _next;
        private readonly ITenantContext _tenantRepository;

        public RequestLoggingMiddleware(RequestDelegate next, ITenantContext tenantRepository, ILogger<RequestLoggingMiddleware> logger)
        {
            _logger = logger;
            _next = next;
            _tenantRepository = tenantRepository;
        }
 
        public async Task InvokeAsync(HttpContext context)
        {
            using (_logger.BeginScope(new TracingLogScope(Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString())))
            using (_logger.BeginScope(new TenantLogScope(_tenantRepository?.TenantId)))
            {
                var httpRequest = context.Request;
                var logState = new IncomingHttpRequestLogState
                {
                    Method = httpRequest?.Method,
                    Target = httpRequest?.Path,
                    UserAgent = httpRequest?.Headers[HeaderNames.UserAgent]
                };
                var sw = Stopwatch.StartNew();
                _logger.LogWithState( LogLevel.Debug, $"Start incoming {logState.Method} to {logState.Target}", logState );
                
                await _next(context);
                var elapsed = sw.ElapsedMilliseconds;
                logState.ServerDuration = elapsed;
                logState.Status = context.Response?.StatusCode;
                _logger.LogWithState( LogLevel.Debug, $"Finished incoming {logState.Method} to {logState.Target} with status code {logState.Status} in {logState.ServerDuration}ms", logState );
            }
        }
    }
}