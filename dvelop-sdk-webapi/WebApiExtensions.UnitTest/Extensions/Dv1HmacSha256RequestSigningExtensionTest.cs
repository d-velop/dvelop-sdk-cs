using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dvelop.Sdk.WebApiExtensions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dvelop.Sdk.WebApiExtensions.UnitTest.Extensions
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dv1HmacSha256RequestSigningExtensionTest
    {
        [TestMethod]
        public async Task TestCalculateDv1HmacSha256SignatureFromExample()
        {
            var x = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "POST",
                Path = "/myapp/dvelop-cloud-lifecycle-event",
                ContentType = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n")),
                Headers =
                {
                    {"x-dv-signature-timestamp","2019-08-09T08:49:42Z"},
                    {"x-dv-signature-algorithm", "DV1-HMAC-SHA256"},
                    {"x-dv-signature-headers", "x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp"},
                    {"Authorization","Bearer 02783453441665bf27aa465cbbac9b98507ae94c54b6be2b1882fe9a05ec104c"}
                }
            };
            var features = x.HttpContext.Features.Get<IHttpRequestFeature>();
            features.RawTarget = "https://acme-apptemplate.service.d-velop.cloud/myapp/dvelop-cloud-lifecycle-event";
            var calculated = await x.CalculateDv1HmacSha256Signature("Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=").ConfigureAwait(false);
            Assert.AreEqual( "02783453441665bf27aa465cbbac9b98507ae94c54b6be2b1882fe9a05ec104c", calculated );
        }

        
        
        [TestMethod]
        public async Task TestCalculateDv1HmacSha256Signature1()
        {
            var x = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "POST",
                Path = "/prod/acme-apptemplatecs/dvelop-cloud-lifecycle-event",
                ContentType = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"type\":\"resubscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}")),
                Headers =
                {
                    {"x-dv-signature-timestamp","2020-04-30T08:16:40Z"},
                    {"x-dv-signature-algorithm", "DV1-HMAC-SHA256"},
                    {"x-dv-signature-headers", "x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp"},
                    {"Authorization","Bearer cc2bfbac52f30ddee41e4475963f8136c60cab678941560538b1281ad2722aac"}
                }
            };
            var features = x.HttpContext.Features.Get<IHttpRequestFeature>();
            features.RawTarget = "https://acme-apptemplate.service.d-velop.cloud/prod/acme-apptemplatecs/dvelop-cloud-lifecycle-event";
            var calculated = await x.CalculateDv1HmacSha256Signature("Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=").ConfigureAwait(false);
            Assert.AreEqual( "cc2bfbac52f30ddee41e4475963f8136c60cab678941560538b1281ad2722aac", calculated);
        }
        
        
        [TestMethod]
        public async Task TestCalculateDv1HmacSha256Signature2()
        {
            var x = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "POST",
                Path = "/prod/acme-apptemplatecs/dvelop-cloud-lifecycle-event",
                ContentType = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n")),
                Headers =
                {
                    {"x-dv-SIGNATURE-timestamp","2019-08-09T08:49:42Z"},
                    {"x-dv-signature-algorithm", "DV1-HMAC-SHA256"},
                    {"x-dv-signature-headers", "x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp"},
                    {"Authorization","Bearer 58b07086ef6d987016d35c8ca2b0c1e48ed1aa8ffe31819402ad8f06c7bd4486"}
                }
            };
            var features = x.HttpContext.Features.Get<IHttpRequestFeature>();
            features.RawTarget = "https://acme-apptemplate.service.d-velop.cloud/prod/acme-apptemplatecs/dvelop-cloud-lifecycle-event";
            var calculated = await x.CalculateDv1HmacSha256Signature("Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=").ConfigureAwait(false);
            Assert.AreEqual( "58b07086ef6d987016d35c8ca2b0c1e48ed1aa8ffe31819402ad8f06c7bd4486", calculated);
        }
        
    }
    
    
}