using System.Collections.Generic;

namespace Dvelop.Sdk.Base.Dto
{
    public class HalJsonDto
    {
        public HalJsonDto()
        {
            _links = new Dictionary<string, RelationDataDto>();
            _embedded = new Dictionary<string, object>();
        }
        // ReSharper disable once InconsistentNaming
        public Dictionary<string, RelationDataDto> _links { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public Dictionary<string, object> _embedded { get; set; }
        
        public bool ShouldSerialize_links()
        {
            return _links?.Count > 0;
        }
        public bool ShouldSerialize_embedded()
        {
            return _embedded?.Count > 0;
        }
    }
}
