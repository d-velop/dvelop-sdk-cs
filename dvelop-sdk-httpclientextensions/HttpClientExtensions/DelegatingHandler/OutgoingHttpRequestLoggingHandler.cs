using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dvelop.Sdk.Logging.Abstractions.Extension;
using Dvelop.Sdk.Logging.Abstractions.Scope;
using Microsoft.Extensions.Logging;

namespace Dvelop.Sdk.HttpClientExtensions.DelegatingHandler
{
    public class OutgoingHttpRequestLoggingHandler : System.Net.Http.DelegatingHandler
    {
        private readonly ILogger<OutgoingHttpRequestLoggingHandler> _logger;

        public OutgoingHttpRequestLoggingHandler(ILogger<OutgoingHttpRequestLoggingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
                var requestLogScope = new OutgoingHttpRequestLogScope
                {
                    Method = request.Method?.ToString(),
                    Target = request.RequestUri?.PathAndQuery,
                };
           
                _logger.LogWithState(LogLevel.Debug, $"Start HTTP {requestLogScope.Method} call to {requestLogScope.Target}",requestLogScope);
                var sw = Stopwatch.StartNew();
                var response = await base.SendAsync(request, cancellationToken);
                var elapsed = sw.ElapsedMilliseconds;
                
                requestLogScope.StatusCode = (int)response.StatusCode;
                requestLogScope.ClientDuration = elapsed;
                _logger.LogWithState(LogLevel.Debug ,$"Finished HTTP {requestLogScope.Method} call to {requestLogScope.Target} with status {(int)response.StatusCode} in {requestLogScope.ClientDuration}", requestLogScope);
                return response;    
            }
        }
}