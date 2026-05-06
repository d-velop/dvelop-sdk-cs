using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Dvelop.Sdk.IdentityProvider.Client;
using Dvelop.Sdk.IdentityProvider.Dto;
using FakeItEasy;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Dvelop.Sdk.IdentityProviderMiddleware.UnitTest
{
    [TestFixture]
    public class IdentityProviderClientTest
    {
        private IdentityProviderClient _unit;
        private FakeHttpMessageHandler _fakeHttpMessageHandler;

        [SetUp]
        public void Setup()
        {
            _fakeHttpMessageHandler = A.Fake<FakeHttpMessageHandler>(o => o.CallsBaseMethods());
            A.CallTo(() => _fakeHttpMessageHandler.Send(A<HttpRequestMessage>.Ignored))
                .Returns(new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new UserDto
                    {
                        Id = "0815-4711",
                        UserName = "user1"
                    }), Encoding.UTF8, "application/json"),
                    Headers =
                    {
                        CacheControl = new CacheControlHeaderValue
                        {
                            MaxAge = TimeSpan.FromHours(1),
                            Private = true
                        }
                    }
                });
            _unit = new IdentityProviderClient(new IdentityProviderClientOptions
            {
                HttpClient = new HttpClient(_fakeHttpMessageHandler),
                SystemBaseUri = new Uri("http://localhost/"),
                TenantInformationCallback = () => new TenantInformation
                    { SystemBaseUri = "http://localhost/", TenantId = "0" }
            });
        }

        [Test]
        public async Task OneSessionShouldOnlyValidateOnce()
        {
            var claimsUser = await _unit.GetClaimsPrincipalAsync("a&1").ConfigureAwait(false);
            Assert.That(claimsUser, Is.Not.Null);

            claimsUser = await _unit.GetClaimsPrincipalAsync("a&1").ConfigureAwait(false);
            Assert.That(claimsUser, Is.Not.Null);

            A.CallTo(() => _fakeHttpMessageHandler.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task TwoSessionsShouldOnlyValidateOncePerSession()
        {
            var claimsUser = await _unit.GetClaimsPrincipalAsync("a&1").ConfigureAwait(false);
            Assert.That(claimsUser, Is.Not.Null);

            claimsUser = await _unit.GetClaimsPrincipalAsync("a&2").ConfigureAwait(false);
            Assert.That(claimsUser, Is.Not.Null);

            claimsUser = await _unit.GetClaimsPrincipalAsync("a&1").ConfigureAwait(false);
            Assert.That(claimsUser, Is.Not.Null);

            claimsUser = await _unit.GetClaimsPrincipalAsync("a&2").ConfigureAwait(false);
            Assert.That(claimsUser, Is.Not.Null);

            A.CallTo(() => _fakeHttpMessageHandler.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappened(2, Times.Exactly);
        }

        [Test]
        public async Task InvalidSessionId()
        {
            var claimsUser = await _unit.GetClaimsPrincipalAsync("\"a&1").ConfigureAwait(false);
            Assert.That(claimsUser, Is.Null);

            var authSessionInfo = await _unit.GetAuthSessionIdFromApiKey("\"a&1").ConfigureAwait(false);
            Assert.That(authSessionInfo, Is.Null);
        }
    }
}
