using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dvelop.Sdk.HttpClientExtensions.Extensions.W3cTracing
{
    public class Wc3TraceHandler: DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Activity.Current != null && request?.Headers != null && !request.Headers.Contains("traceparent"))
            {
                request.Headers.Add("traceparent", Activity.Current.Id);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}