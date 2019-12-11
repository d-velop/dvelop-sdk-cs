using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dvelop.Sdk.Inbound.Dto;

namespace Dvelop.Sdk.Inbound.Client
{
    public class InboundAppClient : IInboundAppClient
    {
        private const string InboundRootUri = "inbound";
        private const string GetImportProcessLinkRelationName = "importProcess";

        private readonly HttpClient _client;

        public InboundAppClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<RootDto> GetRootAsync()
        {
            var response = await _client.GetJsonAsync(new Uri(InboundRootUri, UriKind.Relative));
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsObjectAsync<RootDto>();
        }

        public async Task<ImportProcessDto> GetImportProcessAsync(string id)
        {
            var root = await GetRootAsync();
            if (!root._links.TryGetValue(GetImportProcessLinkRelationName, out var getImportProcessRelationData))
            {
                throw new Exception();
            }
            var getImportProcessUri = getImportProcessRelationData.Href;
            
            var response = await _client.GetJsonAsync(new Uri(getImportProcessUri, UriKind.Relative));
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsObjectAsync<ImportProcessDto>();
        }
    }
}