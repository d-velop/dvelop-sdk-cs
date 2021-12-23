using System.Diagnostics;
using System.Threading.Tasks;
using Dvelop.Sdk.Logging.Abstractions.Scope;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dvelop.Sdk.WebApiExtensions.Middleware
{
    public class W3CTracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<W3CTracingMiddleware> _logger;

        public W3CTracingMiddleware(RequestDelegate next, ILogger<W3CTracingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (Activity.Current == null)
            {
                var activity = new Activity("");

                if (context.Request.Headers.TryGetValue("traceparent", out var traceparent) && traceparent.Count >= 1)
                {
                    activity.SetParentId(traceparent[0]);
                }

                activity.Start();
                Activity.Current = activity;
            }

            using (_logger.BeginScope(new TracingLogScope(Activity.Current?.TraceId.ToString(),
                       Activity.Current?.SpanId.ToString())))
            {
                await _next(context).ConfigureAwait(false);    
            }
        }
    }
}