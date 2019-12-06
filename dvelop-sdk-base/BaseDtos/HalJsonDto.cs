using System.Collections.Generic;

namespace Dvelop.Sdk.Base.Dtos
{
    public class HalJsonDto
    {
        public HalJsonDto()
        {
            _links = new Dictionary<string, RelationDataDto>();
        }
        // ReSharper disable once InconsistentNaming
        public Dictionary<string, RelationDataDto> _links { get; set; }
    }
}
