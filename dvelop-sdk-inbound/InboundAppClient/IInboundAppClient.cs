using System.Threading.Tasks;
using Dvelop.Sdk.Inbound.Dto;

namespace Dvelop.Sdk.Inbound.Client
{
    public interface IInboundAppClient
    {
        Task<RootDto> GetRootAsync();
        Task<ImportProcessDto> GetImportProcessAsync(string id);
    }
}