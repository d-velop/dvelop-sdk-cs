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
        private const string ExampleBody = "{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n";
        
        [TestMethod]
        public async Task TestCalculateDv1HmacSha256Signature()
        {
            var x = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "POST",
                Path = "/myapp/dvelop-cloud-lifecycle-event",
                ContentType = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(ExampleBody)),
                Headers =
                {
                    {"x-dv-signature-timestamp","2019-08-09T08:49:42Z"},
                    {"x-dv-signature-algorithm", "DV1-HMAC-SHA256"},
                    {"x-dv-signature-headers", "x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp"},
                    {"Authorization","Bearer 58b07086ef6d987016d35c8ca2b0c1e48ed1aa8ffe31819402ad8f06c7bd4486"}
                }
            };
            var features = x.HttpContext.Features.Get<IHttpRequestFeature>();
            features.RawTarget = "https://acme-apptemplate.service.d-velop.cloud/prod/acme-apptemplatecs/dvelop-cloud-lifecycle-event";
            var calculated = await x.CalculateDv1HmacSha256Signature("Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=");
            Assert.AreEqual( "58b07086ef6d987016d35c8ca2b0c1e48ed1aa8ffe31819402ad8f06c7bd4486", calculated );
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
                    {"Authorization","Bearer 93e7a60e2209b7c2b2f73ef082a77aacedbfed1aa14077312293d08aa1940b08"}
                }
            };
            var features = x.HttpContext.Features.Get<IHttpRequestFeature>();
            features.RawTarget = "https://acme-apptemplate.service.d-velop.cloud/prod/acme-apptemplatecs/dvelop-cloud-lifecycle-event";
            var calculated = await x.CalculateDv1HmacSha256Signature("Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=");
            Assert.AreEqual( "93e7a60e2209b7c2b2f73ef082a77aacedbfed1aa14077312293d08aa1940b08", calculated);
        }
        
    }
    
    
}