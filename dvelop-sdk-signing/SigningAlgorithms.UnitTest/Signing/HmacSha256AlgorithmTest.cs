using System;
using System.Buffers.Text;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dvelop.Sdk.SigningAlgorithms.UnitTest.Signing
{
    [TestClass]
    public class HmacSha256AlgorithmTest
    {

        private const string Key = "Rg9iJXX0Jkun9u4Rp6no8HTNEdHlfX9aZYbFJ9b6YdQ=";
        
        [DataTestMethod]
        [DataRow("","e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
        [DataRow("Input","36ecb4f8669133ce744c21982ba4abe2ecd7086e1dc2226ccd6f266f3a5005f8")]
        [DataRow("{\"type\":\"subscribe\",\"tenantId\":\"id\",\"baseUri\":\"https://someone.d-velop.cloud\"}\n", "c2a6fefc93b809eeaf2f069504fe8e02b0f3341b3c5e488e6a402ca45301415c")]
        [DataRow("POST\n/myapp/dvelop-cloud-lifecycle-event\n\nx-dv-signature-algorithm:DV1-HMAC-SHA256\nx-dv-signature-headers:x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp\nx-dv-signature-timestamp:2019-08-09T08:49:42Z\n\nc2a6fefc93b809eeaf2f069504fe8e02b0f3341b3c5e488e6a402ca45301415c","fcecaac3dae4d40d6f2a065678f59f4794dfbe8497fe9ca825f737299887ebf4")]
        public void TestSha256(string input, string expected)
        {
            Assert.AreEqual( expected, HmacSha256Algorithm.Sha256( input ));
        }
        
        
        [DataTestMethod]
        [DataRow("","14cec2793d3c3b01e0d5730d79f187141f9a8ce9e9fa1815a73ffb9df6d61bbe")]
        [DataRow("Input","15c8b4a5aedee67aa3eb383f45ed8dde8713a7e2c94e62ad37fc4aaadda8cd3a")]
        [DataRow("fbfcc87ba2b7ae35506a780388bef9eccb25d734e4a90ad75e8da8002d443128", "cc2bfbac52f30ddee41e4475963f8136c60cab678941560538b1281ad2722aac")]
        public void HmacSha256(string input, string expected)
        {
            Assert.AreEqual( expected, HmacSha256Algorithm.HmacSha256( Convert.FromBase64String(Key) , input ));
        }
        
        [DataTestMethod]
        [DataRow("POST\n/myapp/dvelop-cloud-lifecycle-event\n\nx-dv-signature-algorithm:DV1-HMAC-SHA256\nx-dv-signature-headers:x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp\nx-dv-signature-timestamp:2019-08-09T08:49:42Z\n\nc2a6fefc93b809eeaf2f069504fe8e02b0f3341b3c5e488e6a402ca45301415c", "02783453441665bf27aa465cbbac9b98507ae94c54b6be2b1882fe9a05ec104c")]
        public void HmacSha256WithBodyHash(string input, string expected)
        {
            Assert.AreEqual( expected, HmacSha256Algorithm.HmacSha256( Convert.FromBase64String(Key) , HmacSha256Algorithm.Sha256(input) ));
        }
    }
}