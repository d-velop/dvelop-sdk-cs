using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dvelop.Sdk.TenantMiddleware
{
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder app,
            TenantMiddlewareOptions tenantMiddlewareOptions)
        {
            if (tenantMiddlewareOptions == null) throw new ArgumentNullException(nameof(tenantMiddlewareOptions));
            if (tenantMiddlewareOptions.OnTenantIdentified == null) throw new ArgumentNullException(nameof(tenantMiddlewareOptions.OnTenantIdentified));
            if (tenantMiddlewareOptions.DefaultSystemBaseUri != null && !Uri.IsWellFormedUriString(tenantMiddlewareOptions.DefaultSystemBaseUri, UriKind.RelativeOrAbsolute)) throw new ArgumentException("Is no valid URI", nameof(tenantMiddlewareOptions.DefaultSystemBaseUri));
            app.UseMiddleware<TenantMiddleware>(app.ApplicationServices.GetService<ILoggerFactory>(), tenantMiddlewareOptions);
            return app;
        }
    }
}