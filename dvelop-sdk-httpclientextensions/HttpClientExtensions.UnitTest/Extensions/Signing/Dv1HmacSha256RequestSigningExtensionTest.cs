using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dvelop.Sdk.HttpClientExtensions.Extensions.Signing;
using NUnit.Framework;

namespace Dvelop.Sdk.HttpClientExtensions.UnitTest.Extensions.Signing
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class Dv1HmacSha256RequestSigningExtensionTest
    {
        [Test]
        public async Task TestSignWithDv1HmacSha256FromExample()
        {
            var x = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n"),
                RequestUri = new Uri("https://developer.d-velop.cloud/myapp/dvelop-cloud-lifecycle-event"),
                Headers =
                {
                    {"x-dv-signature-timestamp","2019-08-09T08:49:42Z"}
                }
            };
            await x.SignWithDv1HmacSha256("Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=").ConfigureAwait(false);
            Assert.That(x.Headers.Authorization.Parameter, Is.EqualTo("02783453441665bf27aa465cbbac9b98507ae94c54b6be2b1882fe9a05ec104c"));
        }

        [Test]
        public async Task TestAllHeaderPresent()
        {
            var x = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n"),
                RequestUri = new Uri("https://developer.d-velop.cloud/myapp/dvelop-cloud-lifecycle-event")
            };

            await x.SignWithDv1HmacSha256("Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=").ConfigureAwait(false);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(x.Headers.GetValues("x-dv-signature-algorithm").FirstOrDefault(), Is.Not.Null);
                Assert.That(x.Headers.GetValues("x-dv-signature-algorithm").FirstOrDefault(), Is.EqualTo("DV1-HMAC-SHA256"));
                Assert.That(x.Headers.GetValues("x-dv-signature-headers").FirstOrDefault(), Is.Not.Null);
                Assert.That(x.Headers.GetValues("x-dv-signature-headers").FirstOrDefault(), Is.EqualTo("x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp"));
                Assert.That(x.Headers.GetValues("x-dv-signature-timestamp").FirstOrDefault(), Is.Not.Null);
                Assert.That(x.Headers.Authorization?.Scheme, Is.EqualTo("Bearer"));
                Assert.That(x.Headers.Authorization?.Parameter, Is.Not.Null);
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task TestEmptyValidSecret(string secret)
        {
            var x = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n"),
                RequestUri = new Uri("https://developer.d-velop.cloud/myapp/dvelop-cloud-lifecycle-event")
            };
            Assert.ThrowsAsync<ArgumentException>(() => x.SignWithDv1HmacSha256(secret));
        }

        [Test]
        public async Task TestInvalidSecret()
        {
            var x = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n"),
                RequestUri = new Uri("https://developer.d-velop.cloud/myapp/dvelop-cloud-lifecycle-event")
            };
            Assert.ThrowsAsync<FormatException>(() => x.SignWithDv1HmacSha256("not base 64"));
        }
    }
}
