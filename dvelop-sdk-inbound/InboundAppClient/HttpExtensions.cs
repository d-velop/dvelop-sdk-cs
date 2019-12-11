using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dvelop.Sdk.Inbound.Client
{
    internal static class HttpExtensions
    {
        public static async Task<T> ReadAsObjectAsync<T>(this HttpContent content)
        {
            using (var contentStream = await content.ReadAsStreamAsync())
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var responseDto = await JsonSerializer.DeserializeAsync<T>(contentStream, options);
                return responseDto;
            }
        }

        public static async Task<HttpResponseMessage> GetJsonAsync(this HttpClient client, Uri requestUri)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = requestUri
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await client.SendAsync(request);
        }
    }
}