using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dvelop.Sdk.BaseInterfaces;

namespace Dvelop.Sdk.HttpClientExtensions.DelegatingHandler
{
    public class ForwardDvRequestIdHandler: System.Net.Http.DelegatingHandler
    {
        private const string REQUEST_ID_HEADER = "x-dv-request-id";
        
        private readonly IRequestContext _requestContext;
        
        public ForwardDvRequestIdHandler(IRequestContext requestContext)
        {
            _requestContext = requestContext;
        }
        
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request?.Headers == null || !request.Headers.Contains(REQUEST_ID_HEADER))
            {
                return base.SendAsync(request, cancellationToken);
            }
            
            if (!string.IsNullOrEmpty(_requestContext.DvRequestId))
            {
                request?.Headers?.Add(REQUEST_ID_HEADER, _requestContext.DvRequestId);
            } 
            else if(!string.IsNullOrEmpty(_requestContext.W3CTraceId))
            {
                request?.Headers?.Add(REQUEST_ID_HEADER, _requestContext.W3CTraceId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}