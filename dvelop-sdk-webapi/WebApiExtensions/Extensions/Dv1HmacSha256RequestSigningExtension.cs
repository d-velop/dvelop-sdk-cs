using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dvelop.Sdk.SigningAlgorithms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Dvelop.Sdk.WebApiExtensions.Extensions
{
    public static class Dv1HmacSha256RequestSigningExtension
    {
        public static async Task<string> CalculateDv1HmacSha256Signature(this HttpRequest request, string appSecret)
        {
            // This enables replay of the request body, needed to get the body to the controller
            
            var reader = new StreamReader(request.Body, Encoding.UTF8, false, 1024*128, true);
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            
            // Never forget to rewind the stream
            request.Body.Seek(0, SeekOrigin.Begin);

            string signatureHeaders = request.Headers["x-dv-signature-headers"];
            if (signatureHeaders == null)
            {
                return null;
            }

            // See: https://github.com/aws/aws-lambda-dotnet/issues/656
            var requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
            
            var uri =new Uri(requestFeature.RawTarget, UriKind.RelativeOrAbsolute);
            
            var headers = signatureHeaders.Split(',');
            Array.Sort(headers, string.Compare);
            var enumerable = headers.Select(header => $"{header.ToLowerInvariant()}:{request.Headers[header]}");

            var normalizedHeaders = string.Join("\n", enumerable.ToArray());

            var httpVerb = request.Method;
            var resourcePath = uri.IsAbsoluteUri?uri.AbsolutePath:uri.OriginalString;
            
            var queryString = request.QueryString.HasValue?request.QueryString.Value.TrimStart('?'):string.Empty;
            
            var payload = HmacSha256Algorithm.Sha256(body);

            var normalizedRequest = $"{httpVerb}\n{resourcePath}\n{queryString}\n{normalizedHeaders}\n\n{payload}";
            
            var requestHash = HmacSha256Algorithm.Sha256(normalizedRequest);

            var signatureHash = HmacSha256Algorithm.HmacSha256(Convert.FromBase64String(appSecret), requestHash);

            return signatureHash;
        }
    }
}