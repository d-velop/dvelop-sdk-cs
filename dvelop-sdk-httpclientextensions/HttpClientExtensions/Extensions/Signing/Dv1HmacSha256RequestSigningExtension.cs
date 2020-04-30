using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dvelop.Sdk.SigningAlgorithms;

namespace Dvelop.Sdk.HttpClientExtensions.Extensions.Signing
{
    public static class Dv1HmacSha256RequestSigningExtension
    {
        
        public static async Task SignWithDv1HmacSha256(this HttpRequestMessage request, string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new ArgumentException(nameof(secret));
            }
            request.Headers.Add("x-dv-signature-algorithm", "DV1-HMAC-SHA256");
            request.Headers.Add("x-dv-signature-headers", "x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp");
            if (!request.Headers.Contains("x-dv-signature-timestamp"))
            {
                request.Headers.Add("x-dv-signature-timestamp", DateTime.UtcNow.ToString("O"));
            }
            var checksumFromRequest = await CreateChecksumFromRequest(request);
            var checksum = HmacSha256Algorithm.HmacSha256(Convert.FromBase64String(secret), HmacSha256Algorithm.Sha256(checksumFromRequest));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", checksum);
        }

        private static async Task<string> CreateChecksumFromRequest(HttpRequestMessage request)
        {
            var verb = request.Method.Method;
            var path = request.RequestUri.AbsolutePath;
            var query = request.RequestUri.Query.TrimStart('?');
            var headers = $"x-dv-signature-algorithm:DV1-HMAC-SHA256\nx-dv-signature-headers:x-dv-signature-algorithm,x-dv-signature-headers,x-dv-signature-timestamp\nx-dv-signature-timestamp:{request.Headers.GetValues("x-dv-signature-timestamp").FirstOrDefault()}\n";
            var body = await request.Content.ReadAsStringAsync();
            return $"{verb}\n{path}\n{query}\n{headers}\n{HmacSha256Algorithm.Sha256(body)}";
        }
    }
}