using System.Diagnostics;
using Microsoft.AspNetCore.Builder;

namespace Dvelop.Sdk.WebApiExtensions.W3CAdapter
{
    public static class W3CAdapterMiddleware
    {
        
        public static IApplicationBuilder UseW3CActivityMiddleware(this IApplicationBuilder app)
        {
            app.Use(async (httpContext, next) =>
            {
                if (Activity.Current == null)
                {
                    var activity = new Activity("");
 
                    if (httpContext.Request.Headers.TryGetValue("traceparent", out var traceparent) && traceparent.Count >= 1)
                    {
                        activity.SetParentId(traceparent[0]);
                    }
 
                    activity.Start();
                    Activity.Current = activity;
                }
                await next.Invoke().ConfigureAwait(false);
            });

            
            return app;
        }
        
    }
}