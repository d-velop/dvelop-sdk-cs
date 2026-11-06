using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Dvelop.Sdk.TenantMiddleware
{
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder app,
            TenantMiddlewareOptions tenantMiddlewareOptions)
        {
            TenantMiddlewareOptionsValidator.ThrowIfInvalid(tenantMiddlewareOptions);

            app.UseMiddleware<TenantMiddleware>(tenantMiddlewareOptions);
            return app;
        }

        /// <summary>
        /// Uses the <see cref="TenantMiddlewareOptions"/> registered via <see cref="AddTenantMiddleware"/>.
        /// Changes of the options (e.g. a rotated signature secret) are applied at runtime.
        /// </summary>
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            var optionsMonitor = app.ApplicationServices.GetRequiredService<IOptionsMonitor<TenantMiddlewareOptions>>();
            // Pass the monitor explicitly, so ActivatorUtilities does not have to choose between the constructors.
            app.UseMiddleware<TenantMiddleware>(optionsMonitor);
            return app;
        }

        /// <summary>
        /// Registers <see cref="TenantMiddlewareOptions"/> including validation on startup.
        /// The returned <see cref="OptionsBuilder{TOptions}"/> can be used for further configuration,
        /// e.g. <c>.Bind(configuration.GetSection("TenantMiddleware"))</c>.
        /// </summary>
        public static OptionsBuilder<TenantMiddlewareOptions> AddTenantMiddleware(this IServiceCollection services,
            Action<TenantMiddlewareOptions> configure = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IValidateOptions<TenantMiddlewareOptions>, TenantMiddlewareOptionsValidator>());

            var optionsBuilder = services.AddOptions<TenantMiddlewareOptions>();
            if (configure is not null)
                optionsBuilder.Configure(configure);
            optionsBuilder.ValidateOnStart();
            return optionsBuilder;
        }
    }
}
