using Dvelop.Sdk.IdentityProvider.Middleware;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Dvelop.Sdk.IdentityProviderMiddleware.UnitTest
{
    [TestFixture]
    public class IdentityProviderExtensionsTest
    {
        [Test]
        [TestCase(null, null, null)]
        [TestCase("123", null, null)]
        [TestCase("123&abc", null, "123&abc")]
        public void TestGetAuthSessionIdFromBearer(string actualBearer, string actualCookie, string expected)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/some/uri";
            if (actualBearer != null)
            {
                context.Request.Headers["Authorization"] = $"Bearer {actualBearer}";
            }

            var c = context.GetAuthSessionId();
            Assert.That(c, Is.EqualTo(expected));
        }
    }
}
