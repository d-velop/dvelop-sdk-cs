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
        public void TestSha256(string input, string expected)
        {
            Assert.AreEqual( expected, HmacSha256Algorithm.Sha256( input ));
        }
        
        
        [DataTestMethod]
        [DataRow("","14cec2793d3c3b01e0d5730d79f187141f9a8ce9e9fa1815a73ffb9df6d61bbe")]
        [DataRow("Input","15c8b4a5aedee67aa3eb383f45ed8dde8713a7e2c94e62ad37fc4aaadda8cd3a")]
        [DataRow("fbfcc87ba2b7ae35506a780388bef9eccb25d734e4a90ad75e8da8002d443128", "fbfcc87ba2b7ae35506a780388bef9eccb25d734e4a90ad75e8da8002d443128")]
        public void HmacSha256(string input, string expected)
        {
            Assert.AreEqual( expected, HmacSha256Algorithm.HmacSha256( Convert.FromBase64String(Key) , input ));
        }
    }
}