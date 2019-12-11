using Dvelop.Sdk.Base.Dto;

namespace Dvelop.Sdk.Inbound.Dto
{
    public class ImportProcessDto: HalJsonDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImportDateTime { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public int PageCount { get; set; }
    }
}
