using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
