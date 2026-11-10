using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Dvelop.Sdk.TenantMiddleware
{
    public class TenantMiddlewareHandler : DelegatingHandler
    {
        private readonly IOptionsMonitor<TenantMiddlewareOptions> _optionsMonitor;

        public TenantMiddlewareHandler(TenantMiddlewareOptions tenantMiddlewareOptions)
        {
            TenantMiddlewareOptionsValidator.ThrowIfInvalid(tenantMiddlewareOptions);

            _optionsMonitor = new StaticOptionsMonitor<TenantMiddlewareOptions>(tenantMiddlewareOptions);
        }

        /// <summary>
        /// Uses the current value of the monitor for every request, so reloaded options (e.g. a rotated
        /// signature secret) are used without restart.
        /// </summary>
        public TenantMiddlewareHandler(IOptionsMonitor<TenantMiddlewareOptions> tenantMiddlewareOptionsMonitor)
        {
            _optionsMonitor = tenantMiddlewareOptionsMonitor ?? throw new ArgumentNullException(nameof(tenantMiddlewareOptionsMonitor));
            // validate the options now to fail fast on startup and behave like the "legacy" constructor
            TenantMiddlewareOptionsValidator.ThrowIfInvalid(_optionsMonitor.CurrentValue);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Read CurrentValue only once per request, so a reload cannot mix old and new values within one request.
            var tenantMiddlewareOptions = _optionsMonitor.CurrentValue;

            var systemBaseUriFromHeader = request.Headers.Contains(TenantMiddleware.SYSTEM_BASE_URI_HEADER)
                ? request.Headers.GetValues(TenantMiddleware.SYSTEM_BASE_URI_HEADER).First()
                : null;
            var tenantIdFromHeader = request.Headers.Contains(TenantMiddleware.TENANT_ID_HEADER)
                ? request.Headers.GetValues(TenantMiddleware.TENANT_ID_HEADER).First()
                : null;
            var base64Signature = request.Headers.Contains(TenantMiddleware.SIGNATURE_HEADER)
                ? request.Headers.GetValues(TenantMiddleware.SIGNATURE_HEADER).First()
                : null;

            var status = TenantMiddleware.Invoke(tenantMiddlewareOptions, systemBaseUriFromHeader, tenantIdFromHeader, base64Signature);
            if (status != 0)
            {
                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(new HttpResponseMessage(status));
                return tsc.Task;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}