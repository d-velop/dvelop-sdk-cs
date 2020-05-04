using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dvelop.Sdk.HttpClientExtensions.Extensions.Signing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dvelop.Sdk.HttpClientExtensions.UnitTest.Extensions.Signing
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dv1HmacSha256RequestSigningExtensionTest
    {
        [TestMethod]
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
            await x.SignWithDv1HmacSha256("Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=");
            Assert.AreEqual("02783453441665bf27aa465cbbac9b98507ae94c54b6be2b1882fe9a05ec104c", x.Headers.Authorization.Parameter);
        }

        [TestMethod]
        public async Task TestAllHeaderPresent()
        {
            var x = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n"),
                RequestUri = new Uri("https://developer.d-velop.cloud/myapp/dvelop-cloud-lifecycle-event")
            };
            
            await x.SignWithDv1HmacSha256("Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=");
            
            Assert.IsNotNull(x.Headers.GetValues("x-dv-signature-algorithm").FirstOrDefault());
            Assert.AreEqual("DV1-HMAC-SHA256",x.Headers.GetValues("x-dv-signature-algorithm").FirstOrDefault());
            Assert.IsNotNull(x.Headers.GetValues("x-dv-signature-headers").FirstOrDefault());
            Assert.AreEqual("x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp",x.Headers.GetValues("x-dv-signature-headers").FirstOrDefault());
            Assert.IsNotNull(x.Headers.GetValues("x-dv-signature-timestamp").FirstOrDefault());
            Assert.AreEqual( "Bearer", x.Headers.Authorization.Scheme );
            Assert.IsNotNull( x.Headers.Authorization.Parameter );
        }
        
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [ExpectedException( typeof(ArgumentException) )]
        public async Task TestEmptyValidSecret(string secret)
        {
            var x = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n"),
                RequestUri = new Uri("https://developer.d-velop.cloud/myapp/dvelop-cloud-lifecycle-event")
            };
            
            await x.SignWithDv1HmacSha256(secret);
        }
        
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public async Task TestInvalidSecret()
        {
            var x = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n"),
                RequestUri = new Uri("https://developer.d-velop.cloud/myapp/dvelop-cloud-lifecycle-event")
            };
            
            await x.SignWithDv1HmacSha256("not base 64");
        }
    }
}