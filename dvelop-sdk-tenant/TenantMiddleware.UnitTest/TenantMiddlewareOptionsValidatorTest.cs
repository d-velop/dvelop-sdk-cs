using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Dvelop.Sdk.TenantMiddleware.UnitTest
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class TenantMiddlewareOptionsValidatorTest
    {
        [Test, UnitUnderTest(typeof(TenantMiddlewareOptionsValidator))]
        public void ValidOptions_ShouldSucceed()
        {
            var result = new TenantMiddlewareOptionsValidator().Validate(null, new TenantMiddlewareOptions
            {
                OnTenantIdentified = (_, _) => { },
                DefaultSystemBaseUri = "https://default.mydomain.de"
            });

            Assert.That(result.Succeeded, Is.True);
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareOptionsValidator))]
        public void NullDefaultSystemBaseUri_ShouldSucceed()
        {
            var result = new TenantMiddlewareOptionsValidator().Validate(null, new TenantMiddlewareOptions
            {
                OnTenantIdentified = (_, _) => { }
            });

            Assert.That(result.Succeeded, Is.True);
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareOptionsValidator))]
        public void OnTenantIdentifiedIsNull_ShouldFail()
        {
            var result = new TenantMiddlewareOptionsValidator().Validate(null, new TenantMiddlewareOptions());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Failed, Is.True);
                Assert.That(result.FailureMessage, Does.Contain("OnTenantIdentified"));
            }
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareOptionsValidator))]
        public void DefaultSystemBaseUriIsNoValidUri_ShouldFail()
        {
            var result = new TenantMiddlewareOptionsValidator().Validate(null, new TenantMiddlewareOptions
            {
                OnTenantIdentified = (_, _) => { },
                DefaultSystemBaseUri = "http:/"
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Failed, Is.True);
                Assert.That(result.FailureMessage, Does.Contain("DefaultSystemBaseUri"));
            }
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareOptionsValidator))]
        public void OptionsAreNull_ShouldFail()
        {
            var result = new TenantMiddlewareOptionsValidator().Validate(null, null);

            Assert.That(result.Failed, Is.True);
        }
    }
}
