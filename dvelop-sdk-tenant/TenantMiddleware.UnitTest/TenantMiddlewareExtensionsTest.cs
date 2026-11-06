using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Dvelop.Sdk.TenantMiddleware.UnitTest
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class TenantMiddlewareExtensionsTest
    {
        [Test, UnitUnderTest(typeof(TenantMiddlewareExtensions))]
        public void TenantMiddlewareOptionsIsNull_ShouldThrowException()
        {
            Action useMiddleware = () => new AppBuilderStub().UseTenantMiddleware(null);
            var ex = Assert.Throws<ArgumentNullException>(() => useMiddleware());
            Assert.That(ex.Message, Does.Contain("tenantMiddlewareOptions"));
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareExtensions))]
        public void OnTenantIdentifiedCallbackIsNull_ShouldThrowException()
        {
            Action useMiddleware = () => new AppBuilderStub().UseTenantMiddleware(new TenantMiddlewareOptions { OnTenantIdentified = null });
            var ex = Assert.Throws<ArgumentNullException>(() => useMiddleware());
            Assert.That(ex.Message, Does.Contain("OnTenantIdentified"));
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareExtensions))]
        public void DefaultSystemBaseUriIsNoValidUri_ShouldThrowException()
        {
            Action useMiddleware = () => new AppBuilderStub().UseTenantMiddleware(
                new TenantMiddlewareOptions
                {
                    OnTenantIdentified = (a, b) => { },
                    DefaultSystemBaseUri = "http:/"
                });
            var ex = Assert.Throws<ArgumentException>(() => useMiddleware());
            Assert.That(ex.Message, Does.Contain("DefaultSystemBaseUri"));
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareExtensions))]
        public void AddTenantMiddleware_ShouldRegisterValidatorOnce()
        {
            var services = new ServiceCollection();
            services.AddTenantMiddleware(o => o.OnTenantIdentified = (_, _) => { });
            services.AddTenantMiddleware();

            using var provider = services.BuildServiceProvider();
            var validators = provider.GetServices<IValidateOptions<TenantMiddlewareOptions>>().ToList();

            Assert.That(validators, Has.Count.EqualTo(1).And.All.InstanceOf<TenantMiddlewareOptionsValidator>());
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareExtensions))]
        public void AddTenantMiddlewareWithoutOnTenantIdentified_ShouldFailValidation()
        {
            var services = new ServiceCollection();
            services.AddTenantMiddleware(o => o.DefaultTenantId = "0");

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<TenantMiddlewareOptions>>();

            var ex = Assert.Throws<OptionsValidationException>(() => _ = monitor.CurrentValue);
            Assert.That(ex.Message, Does.Contain("OnTenantIdentified"));
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareExtensions))]
        public void UseTenantMiddlewareWithoutAddTenantMiddleware_ShouldThrowException()
        {
            using var provider = new ServiceCollection().BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => new ApplicationBuilder(provider).UseTenantMiddleware());
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareExtensions))]
        public async Task UseTenantMiddleware_ShouldUseOptionsFromServiceProvider()
        {
            string identifiedTenantId = null;
            var services = new ServiceCollection();
            services.AddTenantMiddleware(o =>
            {
                o.DefaultTenantId = "4711";
                o.OnTenantIdentified = (tenantId, _) => identifiedTenantId = tenantId;
            });
            using var provider = services.BuildServiceProvider();

            var app = new ApplicationBuilder(provider);
            app.UseTenantMiddleware();
            var pipeline = app.Build();
            await pipeline(new DefaultHttpContext()).ConfigureAwait(false);

            Assert.That(identifiedTenantId, Is.EqualTo("4711"));
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareExtensions))]
        public void AddTenantMiddlewareWithBind_ShouldReloadSignatureSecretKey()
        {
            var oldKey = new byte[] { 1, 2, 3, 4 };
            var newKey = new byte[] { 5, 6, 7, 8 };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["TenantMiddleware:SignatureSecretKey"] = Convert.ToBase64String(oldKey),
                    ["TenantMiddleware:DefaultTenantId"] = "0"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddTenantMiddleware(o => o.OnTenantIdentified = (_, _) => { })
                .Bind(configuration.GetSection("TenantMiddleware"));
            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<TenantMiddlewareOptions>>();

            var beforeReload = monitor.CurrentValue;
            configuration["TenantMiddleware:SignatureSecretKey"] = Convert.ToBase64String(newKey);
            configuration.Reload();
            var afterReload = monitor.CurrentValue;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(beforeReload.SignatureSecretKey, Is.EqualTo(oldKey));
                Assert.That(afterReload.SignatureSecretKey, Is.EqualTo(newKey));
                Assert.That(afterReload.DefaultTenantId, Is.EqualTo("0"));
                Assert.That(afterReload.OnTenantIdentified, Is.Not.Null);
            }
        }
    }

    internal class AppBuilderStub : IApplicationBuilder
    {
        public IApplicationBuilder Use(object middleware, params object[] args)
        {
            return this;
        }

        public object Build(Type returnType)
        {
            throw new NotImplementedException();
        }

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            throw new NotImplementedException();
        }

        public IApplicationBuilder New()
        {
            throw new NotImplementedException();
        }

        public RequestDelegate Build()
        {
            throw new NotImplementedException();
        }

        public IServiceProvider ApplicationServices { get; set; }

        public IFeatureCollection ServerFeatures { get; }

        public IDictionary<string, object> Properties => throw new NotImplementedException();
    }
}
