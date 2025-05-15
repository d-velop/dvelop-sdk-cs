using System;
using System.Security.Claims;
using Dvelop.Sdk.IdentityProvider.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dvelop.Sdk.IdentityProviderMiddleware.UnitTest
{
    [TestClass]
    public class IdentityProviderSessionStoreTest
    {
        private IdentityProviderSessionStore _unit;
        private Mock<TimeProvider> _clock;

        [TestInitialize]
        public void Setup()
        {
            _clock = new Mock<TimeProvider> {CallBase = true};
            _unit = new IdentityProviderSessionStore(_clock.Object, 2);
        }
        
        
        [TestMethod]
        public void GetUnknownCookieReturnsNull()
        {
            Assert.IsNull(_unit.GetPrincipal("a&1"));
        }
        
        [TestMethod]
        public void GetKnownCookieShouldReturnPrincipal()
        {
            var cookie = "a&1";
            var claimsPrincipal = new ClaimsPrincipal();
            _unit.SetPrincipal(cookie, DateTime.Now.AddHours(1), claimsPrincipal );
            Assert.AreEqual(claimsPrincipal, _unit.GetPrincipal(cookie));
        }
        
        
        [TestMethod]
        public void GetUnknownCookieShouldReturnPrincipal()
        {
            var user1 = "a&1";
            var user2 = "b&1";
            var claimsPrincipal = new ClaimsPrincipal();
            _unit.SetPrincipal(user1, DateTime.Now.AddHours(1), claimsPrincipal );
            Assert.IsNull(_unit.GetPrincipal(user2 ));
        }
        
                
        [TestMethod]
        public void GetSecondSessionUserCookieShouldReturnPrincipal()
        {
            var user1 = "a&1";
            var user2 = "a&2";
            var claimsPrincipal = new ClaimsPrincipal();
            _unit.SetPrincipal(user1, DateTime.Now.AddHours(1), claimsPrincipal );
            Assert.AreEqual(claimsPrincipal, _unit.GetPrincipal(user1));
            Assert.IsNull(_unit.GetPrincipal(user2));
        }

        [TestMethod]
        public void GetExpiredItemShouldReturnNull()
        {
            var now = DateTimeOffset.UtcNow;
            _clock.SetupSequence(c => c.GetUtcNow())
                .Returns(now)
                .Returns(now.AddMinutes(61));   
            
            const string user1 = "a&1";
            var claimsPrincipal = new ClaimsPrincipal();
            var expire = now.AddHours(1);
            Console.WriteLine( user1 + " -> " + expire);
            
            _unit.SetPrincipal(user1,now.AddHours(1),claimsPrincipal);
            
            Assert.IsNotNull(_unit.GetPrincipal(user1));
            Assert.IsNull(_unit.GetPrincipal(user1));
        }
        
        [TestMethod]
        public void GetNonExpiredItemShouldReturnAndRefresh()
        {
            var now = DateTimeOffset.UtcNow;
            _clock.SetupSequence(c => c.GetUtcNow())
                // .Returns(now)                    // SET 
                .Returns(now.AddMinutes(56))     // GET a&1
                .Returns(now.AddMinutes(61));    // GET a&1
            
            const string user1 = "a&1";
            var claimsPrincipal1 = new ClaimsPrincipal();
            
            _unit.SetPrincipal(user1, now.AddHours(1), claimsPrincipal1);
            
            // 56 minutes later
            Assert.IsNotNull(_unit.GetPrincipal(user1, out var doRefresh));
            Assert.IsTrue(doRefresh);
            
            // 61 minutes later
            Assert.IsNull(_unit.GetPrincipal(user1, out doRefresh));
            Assert.IsFalse(doRefresh);
        }

        [TestMethod]
        public void GetNonExpiredItemShouldReturn()
        {
            var now = DateTimeOffset.UtcNow;
            _clock.SetupSequence(c => c.GetUtcNow())
                .Returns(now).Returns(now)      // SET 
                .Returns(now.AddMinutes(61))    // GET a&1
                .Returns(now.AddMinutes(61));   // GET b&1
            
            const string user1 = "a&1";
            var claimsPrincipal1 = new ClaimsPrincipal();
            const string user2 = "b&1";
            var claimsPrincipal2 = new ClaimsPrincipal();
            
            _unit.SetPrincipal(user1,now.AddHours(1),claimsPrincipal1);
            _unit.SetPrincipal(user2,now.AddHours(2),claimsPrincipal2);
            
            // 59 minutes later
            Assert.IsNotNull(_unit.GetPrincipal(user1));
            Assert.IsNotNull(_unit.GetPrincipal(user2));
            
            // 61 minutes later
            Assert.IsNull(_unit.GetPrincipal(user1));
            Assert.IsNotNull(_unit.GetPrincipal(user2));
        }
    }
}