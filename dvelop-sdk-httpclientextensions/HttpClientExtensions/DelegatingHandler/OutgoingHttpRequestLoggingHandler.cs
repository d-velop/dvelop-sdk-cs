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
                var x = new OutgoingHttpRequestLogScope
                {
                    Method = request.Method?.ToString(),
                    Target = request.RequestUri?.PathAndQuery,
                };
           
                _logger.LogWithState(LogLevel.Debug, $"Start HTTP {x.Method} call to {x.Target}",x);
                var sw = Stopwatch.StartNew();
                var response = await base.SendAsync(request, cancellationToken);
                var elapsed = sw.ElapsedMilliseconds;
                
                x.StatusCode = (int)response.StatusCode;
                x.Elapsed = elapsed;
                _logger.LogWithState(LogLevel.Debug ,$"Finished HTTP {x.Method} call to {x.Target} with status {(int)response.StatusCode} in ", x);
                return response;    
            }
        }
}