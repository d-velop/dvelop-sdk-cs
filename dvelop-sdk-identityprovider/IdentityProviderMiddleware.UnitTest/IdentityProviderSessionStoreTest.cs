using System;
using System.Security.Claims;
using Dvelop.Sdk.IdentityProvider.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dvelop.Sdk.IdentityProviderMiddleware.UnitTest
{
    [TestClass]
    public class IdentityProviderSessionStoreTest
    {
        private IdentityProviderSessionStore unit;

        [TestInitialize]
        public void Setup()
        {
            unit = new IdentityProviderSessionStore();
        }
        
        
        [TestMethod]
        public void GetUnknownCookieReturnsNull()
        {
            Assert.IsNull(unit.GetPrincipal("a&1"));
        }
        
        [TestMethod]
        public void GetKnownCookieShouldReturnPrincipal()
        {
            var cookie = "a&1";
            var claimsPrincipal = new ClaimsPrincipal();
            unit.SetPrincipal(cookie, DateTime.Now.AddHours(1), claimsPrincipal );
            Assert.AreEqual(claimsPrincipal, unit.GetPrincipal(cookie));
        }
        
        
        [TestMethod]
        public void GetUnknownCookieShouldReturnPrincipal()
        {
            var user1 = "a&1";
            var user2 = "b&1";
            var claimsPrincipal = new ClaimsPrincipal();
            unit.SetPrincipal(user1, DateTime.Now.AddHours(1), claimsPrincipal );
            Assert.IsNull(unit.GetPrincipal(user2 ));
        }
        
                
        [TestMethod]
        public void GetSecondSessionUserCookieShouldReturnPrincipal()
        {
            var user1 = "a&1";
            var user2 = "a&2";
            var claimsPrincipal = new ClaimsPrincipal();
            unit.SetPrincipal(user1, DateTime.Now.AddHours(1), claimsPrincipal );
            Assert.AreEqual(claimsPrincipal, unit.GetPrincipal(user1));
            Assert.IsNull(unit.GetPrincipal(user2));
        }
    }
}