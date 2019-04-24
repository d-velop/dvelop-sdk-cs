using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace dvelop.TenantMiddleware
{
    public class TenantMiddlewareHandler : DelegatingHandler
    {
        private readonly TenantMiddlewareOptions _tenantMiddlewareOptions;

        public TenantMiddlewareHandler(TenantMiddlewareOptions tenantMiddlewareOptions)
        {
            if (tenantMiddlewareOptions == null) throw new ArgumentNullException(nameof(tenantMiddlewareOptions));
            if (tenantMiddlewareOptions.OnTenantIdentified == null)
                throw new ArgumentNullException(nameof(tenantMiddlewareOptions.OnTenantIdentified));
            if (tenantMiddlewareOptions.DefaultSystemBaseUri != null &&
                !Uri.IsWellFormedUriString(tenantMiddlewareOptions.DefaultSystemBaseUri, UriKind.RelativeOrAbsolute))
                throw new ArgumentException("Is no valid URI", nameof(tenantMiddlewareOptions.DefaultSystemBaseUri));

            _tenantMiddlewareOptions = tenantMiddlewareOptions;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var systemBaseUriFromHeader = request.Headers.Contains(TenantMiddleware.SYSTEM_BASE_URI_HEADER)
                ? request.Headers.GetValues(TenantMiddleware.SYSTEM_BASE_URI_HEADER).First()
                : null;
            var tenantIdFromHeader = request.Headers.Contains(TenantMiddleware.TENANT_ID_HEADER)
                ? request.Headers.GetValues(TenantMiddleware.TENANT_ID_HEADER).First()
                : null;
            var base64Signature = request.Headers.Contains(TenantMiddleware.SIGNATURE_HEADER)
                ? request.Headers.GetValues(TenantMiddleware.SIGNATURE_HEADER).First()
                : null;

            var status = TenantMiddleware.Invoke(_tenantMiddlewareOptions, systemBaseUriFromHeader, tenantIdFromHeader, base64Signature);
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