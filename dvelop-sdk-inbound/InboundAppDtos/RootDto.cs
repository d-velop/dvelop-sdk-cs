using Dvelop.Sdk.Base.Dto;

namespace Dvelop.Sdk.Inbound.Dto
{
    public class RootDto: HalJsonDto
    {
        public string Name { get; set; }
        public VersionDto Version { get; set; }
    }
}