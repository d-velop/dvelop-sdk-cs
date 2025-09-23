using Dvelop.Sdk.IdentityProvider.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dvelop.Sdk.IdentityProviderMiddleware.UnitTest
{
    [TestClass]
    public class IdentityProviderExtensionsTest
    {
        [TestMethod]
        [DataRow( null, null, null )]
        [DataRow( "123", null, null )]
        [DataRow( "123&abc", null, "123&abc" )]
        public void TestGetAuthSessionIdFromBearer(string actualBearer, string actualCookie, string expected)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/some/uri";
            if (actualBearer != null)
            {
                context.Request.Headers["Authorization"]=$"Bearer {actualBearer}";
            }

            var c = context.GetAuthSessionId();
            Assert.AreEqual(expected, c);
        }
    }
}