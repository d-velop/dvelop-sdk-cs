using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dvelop.Sdk.WebApiExtensions.Middleware
{
    public class W3CTracingMiddleware
    {
        private readonly RequestDelegate _next;

        public W3CTracingMiddleware(RequestDelegate next)
        {
            _next = next;
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

            await _next(context).ConfigureAwait(false);
        }
    }
}