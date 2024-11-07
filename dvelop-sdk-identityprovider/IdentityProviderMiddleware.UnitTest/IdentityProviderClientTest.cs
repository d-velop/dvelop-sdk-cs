using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Dvelop.Sdk.IdentityProvider.Client;
using Dvelop.Sdk.IdentityProvider.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dvelop.Sdk.IdentityProviderMiddleware.UnitTest
{
    [TestClass]
    public class IdentityProviderClientTest
    {
        private IdentityProviderClient _unit;
        private Mock<FakeHttpMessageHandler> _fakeHttpMessageHandler;

        [TestInitialize]
        public void Setup()
        {
            _fakeHttpMessageHandler = new Mock<FakeHttpMessageHandler>
            {
                CallBase = true
            };
            _fakeHttpMessageHandler
                .Setup(h => h.Send(It.IsAny<HttpRequestMessage>()))
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
                HttpClient = new HttpClient(_fakeHttpMessageHandler.Object),
                TenantInformationCallback = () => new TenantInformation
                    { SystemBaseUri = "http://localhost/", TenantId = "0" }
            });
        }

        [TestMethod]
        public async Task OneSessionShouldOnlyValidateOnce()
        {
            var claimsUser = await _unit.GetClaimsPrincipalAsync("a&1").ConfigureAwait(false);
            Assert.IsNotNull(claimsUser);

            claimsUser = await _unit.GetClaimsPrincipalAsync("a&1").ConfigureAwait(false);
            Assert.IsNotNull(claimsUser);

            _fakeHttpMessageHandler.Verify(h => h.Send(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [TestMethod]
        public async Task TwoSessionsShouldOnlyValidateOncePerSession()
        {
            var claimsUser = await _unit.GetClaimsPrincipalAsync("a&1").ConfigureAwait(false);
            Assert.IsNotNull(claimsUser);

            claimsUser = await _unit.GetClaimsPrincipalAsync("a&2").ConfigureAwait(false);
            Assert.IsNotNull(claimsUser);

            claimsUser = await _unit.GetClaimsPrincipalAsync("a&1").ConfigureAwait(false);
            Assert.IsNotNull(claimsUser);

            claimsUser = await _unit.GetClaimsPrincipalAsync("a&2").ConfigureAwait(false);
            Assert.IsNotNull(claimsUser);

            _fakeHttpMessageHandler.Verify(h => h.Send(It.IsAny<HttpRequestMessage>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task InvalidSessionId()
        {
            var claimsUser = await _unit.GetClaimsPrincipalAsync("\"a&1").ConfigureAwait(false);
            Assert.IsNull(claimsUser);

            var authSessionInfo = await _unit.GetAuthSessionIdFromApiKey("\"a&1").ConfigureAwait(false);
            Assert.IsNull(authSessionInfo);
        }
    }
}