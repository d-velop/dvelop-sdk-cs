using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
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
            Action useMiddleware = () => new TenantMiddlewareHandler((TenantMiddlewareOptions)null);
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

        [Test, UnitUnderTest(typeof(TenantMiddlewareHandler))]
        public void OptionsMonitorIsNull_ShouldThrowException()
        {
            Action useMiddleware = () => new TenantMiddlewareHandler((IOptionsMonitor<TenantMiddlewareOptions>)null);
            var ex = Assert.Throws<ArgumentNullException>(() => useMiddleware());
            Assert.That(ex.Message, Does.Contain("tenantMiddlewareOptionsMonitor"));
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareHandler))]
        public void OptionsMonitorWithInvalidOptions_ShouldThrowException()
        {
            var monitor = A.Fake<IOptionsMonitor<TenantMiddlewareOptions>>();
            A.CallTo(() => monitor.CurrentValue).Returns(new TenantMiddlewareOptions { OnTenantIdentified = null });

            var ex = Assert.Throws<ArgumentNullException>(() => new TenantMiddlewareHandler(monitor));
            Assert.That(ex.Message, Does.Contain("OnTenantIdentified"));
        }

        [Test, UnitUnderTest(typeof(TenantMiddlewareHandler))]
        public async Task OptionsMonitorWithRotatedSignatureKey_ShouldUseCurrentKey()
        {
            const string systemBaseUri = "https://sample.mydomain.de";
            var oldKey = Enumerable.Range(1, 32).Select(i => (byte)i).ToArray();
            var newKey = Enumerable.Range(101, 32).Select(i => (byte)i).ToArray();

            var options = new TenantMiddlewareOptions { SignatureSecretKey = oldKey, OnTenantIdentified = (_, _) => { } };
            var monitor = A.Fake<IOptionsMonitor<TenantMiddlewareOptions>>();
            A.CallTo(() => monitor.CurrentValue).ReturnsLazily(() => options);

            using var invoker = new HttpMessageInvoker(new TenantMiddlewareHandler(monitor) { InnerHandler = new OkHandler() });

            async Task<HttpStatusCode> Send(byte[] signingKey)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
                request.Headers.Add("x-dv-baseuri", systemBaseUri);
                using var mac = new HMACSHA256(signingKey);
                request.Headers.Add("x-dv-sig-1", Convert.ToBase64String(mac.ComputeHash(Encoding.ASCII.GetBytes(systemBaseUri))));
                using var response = await invoker.SendAsync(request, CancellationToken.None).ConfigureAwait(false);
                return response.StatusCode;
            }

            var beforeRotation = await Send(oldKey).ConfigureAwait(false);
            options = new TenantMiddlewareOptions { SignatureSecretKey = newKey, OnTenantIdentified = (_, _) => { } };
            var oldKeyAfterRotation = await Send(oldKey).ConfigureAwait(false);
            var newKeyAfterRotation = await Send(newKey).ConfigureAwait(false);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(beforeRotation, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(oldKeyAfterRotation, Is.EqualTo(HttpStatusCode.Forbidden));
                Assert.That(newKeyAfterRotation, Is.EqualTo(HttpStatusCode.OK));
            }
            // once for the validation in the constructor, then once per request
            A.CallTo(() => monitor.CurrentValue).MustHaveHappened(4, Times.Exactly);
        }

        private class OkHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
