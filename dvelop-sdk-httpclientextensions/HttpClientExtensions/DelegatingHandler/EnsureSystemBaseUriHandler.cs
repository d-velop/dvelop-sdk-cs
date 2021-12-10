using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dvelop.Sdk.BaseInterfaces;

namespace Dvelop.Sdk.HttpClientExtensions.DelegatingHandler
{
    public class EnsureSystemBaseUriHandler: System.Net.Http.DelegatingHandler
    {
        private readonly ITenantContext _tenantContext;

        public EnsureSystemBaseUriHandler(ITenantContext tenantContext)
        {
            _tenantContext = tenantContext;
        }
        
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.RequestUri = new Uri(_tenantContext.SystemBaseUri, request.RequestUri.PathAndQuery);
            return base.SendAsync(request, cancellationToken);
        }
    }
}