using System;
using System.Security.Claims;
using Dvelop.Sdk.IdentityProvider.Client;
using FakeItEasy;
using NUnit.Framework;

namespace Dvelop.Sdk.IdentityProviderMiddleware.UnitTest
{
    [TestFixture]
    public class IdentityProviderSessionStoreTest
    {
        private IdentityProviderSessionStore _unit;
        private TimeProvider _clock;

        [SetUp]
        public void Setup()
        {
            _clock = A.Fake<TimeProvider>(o => o.CallsBaseMethods());
            _unit = new IdentityProviderSessionStore(_clock, 2);
        }


        [Test]
        public void GetUnknownCookieReturnsNull()
        {
            Assert.That(_unit.GetPrincipal("a&1"), Is.Null);
        }

        [Test]
        public void GetKnownCookieShouldReturnPrincipal()
        {
            var cookie = "a&1";
            var claimsPrincipal = new ClaimsPrincipal();
            _unit.SetPrincipal(cookie, DateTime.Now.AddHours(1), claimsPrincipal);
            Assert.That(_unit.GetPrincipal(cookie), Is.EqualTo(claimsPrincipal));
        }


        [Test]
        public void GetUnknownCookieShouldReturnPrincipal()
        {
            var user1 = "a&1";
            var user2 = "b&1";
            var claimsPrincipal = new ClaimsPrincipal();
            _unit.SetPrincipal(user1, DateTime.Now.AddHours(1), claimsPrincipal);
            Assert.That(_unit.GetPrincipal(user2), Is.Null);
        }


        [Test]
        public void GetSecondSessionUserCookieShouldReturnPrincipal()
        {
            var user1 = "a&1";
            var user2 = "a&2";
            var claimsPrincipal = new ClaimsPrincipal();
            _unit.SetPrincipal(user1, DateTime.Now.AddHours(1), claimsPrincipal);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_unit.GetPrincipal(user1), Is.EqualTo(claimsPrincipal));
                Assert.That(_unit.GetPrincipal(user2), Is.Null);
            }
        }

        [Test]
        public void GetExpiredItemShouldReturnNull()
        {
            var now = DateTimeOffset.UtcNow;
            A.CallTo(() => _clock.GetUtcNow()).ReturnsNextFromSequence(now, now, now.AddMinutes(61));

            const string user1 = "a&1";
            var claimsPrincipal = new ClaimsPrincipal();
            var expire = now.AddHours(1);
            Console.WriteLine(user1 + " -> " + expire);

            _unit.SetPrincipal(user1, now.AddHours(1), claimsPrincipal);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_unit.GetPrincipal(user1), Is.Not.Null);
                Assert.That(_unit.GetPrincipal(user1), Is.Null);
            }
        }

        [Test]
        public void GetNonExpiredItemWithExpireTimeOverFiveMinutesShouldReturnAndRefresh()
        {
            var now = DateTimeOffset.UtcNow;
            A.CallTo(() => _clock.GetUtcNow()).ReturnsNextFromSequence(
                now,               // SET
                now.AddMinutes(56), // GET a&1
                now.AddMinutes(61)  // GET a&1
            );

            const string user1 = "a&1";
            var claimsPrincipal1 = new ClaimsPrincipal();

            _unit.SetPrincipal(user1, now.AddHours(1), claimsPrincipal1);

            bool doRefresh;

            // 56 minutes later
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_unit.GetPrincipal(user1, out doRefresh), Is.Not.Null);
                Assert.That(doRefresh, Is.True);
            }

            // 61 minutes later
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_unit.GetPrincipal(user1, out doRefresh), Is.Null);
                Assert.That(doRefresh, Is.False);
            }
        }

        [Test]
        public void GetNonExpiredItemWithExpireTimeUnderFiveMinutesShouldReturnAndRefresh()
        {
            var now = DateTimeOffset.UtcNow;
            A.CallTo(() => _clock.GetUtcNow()).ReturnsNextFromSequence(
                now,               // SET
                now.AddMinutes(17), // GET a&1
                now.AddMinutes(21)  // GET a&1
            );

            const string user1 = "a&1";
            var claimsPrincipal1 = new ClaimsPrincipal();

            _unit.SetPrincipal(user1, now.AddMinutes(20), claimsPrincipal1);

            bool doRefresh;

            // 17 minutes later
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_unit.GetPrincipal(user1, out doRefresh), Is.Not.Null);
                Assert.That(doRefresh, Is.True);
            }

            // 21 minutes later
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_unit.GetPrincipal(user1, out doRefresh), Is.Null);
                Assert.That(doRefresh, Is.False);
            }
        }

        [Test]
        public void GetNonExpiredItemShouldReturn()
        {
            var now = DateTimeOffset.UtcNow;
            A.CallTo(() => _clock.GetUtcNow()).ReturnsNextFromSequence(
                now, now,           // SET
                now, now,           // GET
                now.AddMinutes(61), // GET a&1
                now.AddMinutes(61)  // GET b&1
            );

            const string user1 = "a&1";
            var claimsPrincipal1 = new ClaimsPrincipal();
            const string user2 = "b&1";
            var claimsPrincipal2 = new ClaimsPrincipal();

            _unit.SetPrincipal(user1, now.AddHours(1), claimsPrincipal1);
            _unit.SetPrincipal(user2, now.AddHours(2), claimsPrincipal2);

            // 59 minutes later
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_unit.GetPrincipal(user1), Is.Not.Null);
                Assert.That(_unit.GetPrincipal(user2), Is.Not.Null);
            }

            // 61 minutes later
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_unit.GetPrincipal(user1), Is.Null);
                Assert.That(_unit.GetPrincipal(user2), Is.Not.Null);
            }
        }
    }
}
