using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Dvelop.Sdk.TenantMiddleware.UnitTest
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class TenantMiddlewareHandlerTest
    {
        [Test, UnitUnderTest(typeof(TenantMiddlewareHandler))]
        public void TenantMiddlewareOptionsIsNull_ShouldThrowException()
        {
            Action useMiddleware = () => new TenantMiddlewareHandler(null);
            var ex = Assert.Throws<ArgumentNullException>(() => useMiddleware());
            Assert.That(ex.Message, Does.Contain("tenantMiddlewareOptions"));
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareHandler))]
        public void OnTenantIdentifiedCallbackIsNull_ShouldThrowException()
        {
            Action useMiddleware = () => new TenantMiddlewareHandler(new TenantMiddlewareOptions { OnTenantIdentified = null });
            var ex = Assert.Throws<ArgumentNullException>(() => useMiddleware());
            Assert.That(ex.Message, Does.Contain("OnTenantIdentified"));
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareHandler))]
        public void DefaultSystemBaseUriIsNoValidUri_ShouldThrowException()
        {
            Action useMiddleware = () => new TenantMiddlewareHandler(new TenantMiddlewareOptions
            {
                OnTenantIdentified = (a, b) => { },
                DefaultSystemBaseUri = "http:/"
            });
            var ex = Assert.Throws<ArgumentException>(() => useMiddleware());
            Assert.That(ex.Message, Does.Contain("DefaultSystemBaseUri"));
        }
    }
}
