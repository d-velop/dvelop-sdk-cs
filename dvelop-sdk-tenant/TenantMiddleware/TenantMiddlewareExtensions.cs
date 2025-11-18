using System;
using Microsoft.AspNetCore.Builder;

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

            app.UseMiddleware<TenantMiddleware>(tenantMiddlewareOptions);
            return app;
        }

        private static readonly IgnoreTenantMiddlewareAttribute _ignoreTenantMiddleware = new IgnoreTenantMiddlewareAttribute();

        public static TBuilder IgnoreTenantMiddleware<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                endpointBuilder.Metadata.Add(_ignoreTenantMiddleware);
            });
            return builder;
        }
    }
}