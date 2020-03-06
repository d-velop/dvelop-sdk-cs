using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dvelop.Sdk.TenantMiddleware.UnitTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TenantMiddlewareHandlerTest
    {
        [TestMethod, UnitUnderTest(typeof(TenantMiddlewareHandler))]
        public void TenantMiddlewareOptionsIsNull_ShouldThrowException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action useMiddleware = () => new TenantMiddlewareHandler(null);
            useMiddleware.ShouldThrow<ArgumentNullException>().WithMessage("*tenantMiddlewareOptions*");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddlewareHandler))]
        public void OnTenantIdentifiedCallbackIsNull_ShouldThrowException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action useMiddleware = () => new TenantMiddlewareHandler(new TenantMiddlewareOptions { OnTenantIdentified = null });
            useMiddleware.ShouldThrow<ArgumentNullException>().WithMessage("*OnTenantIdentified*");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddlewareHandler))]
        public void DefaultSystemBaseUriIsNoValidUri_ShouldThrowException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action useMiddleware = () => new TenantMiddlewareHandler(new TenantMiddlewareOptions
            {
                OnTenantIdentified = (a, b) => { },
                DefaultSystemBaseUri = "http:/"
            });
            useMiddleware.ShouldThrow<ArgumentException>().WithMessage("*DefaultSystemBaseUri*");
        }
    }
}