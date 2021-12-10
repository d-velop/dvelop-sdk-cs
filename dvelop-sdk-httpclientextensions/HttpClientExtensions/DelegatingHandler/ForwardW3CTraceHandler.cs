using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dvelop.Sdk.BaseInterfaces;

namespace Dvelop.Sdk.HttpClientExtensions.DelegatingHandler
{
    public class ForwardW3CTraceHandler : System.Net.Http.DelegatingHandler
    {
        private const string TRACEPARENT = "traceparent";
        
        private readonly IRequestContext _requestContext;

        public ForwardW3CTraceHandler(IRequestContext requestContext)
        {
            _requestContext = requestContext;
        }
        
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_requestContext.W3CTraceId) && request?.Headers != null && !request.Headers.Contains(TRACEPARENT))
            {
                request.Headers.Add(TRACEPARENT, _requestContext.W3CTraceId);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}