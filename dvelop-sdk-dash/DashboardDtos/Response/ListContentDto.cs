using System.Collections.Generic;
using Dvelop.Sdk.Dashboard.Dto.Response.Types;
using Newtonsoft.Json;

namespace Dvelop.Sdk.Dashboard.Dto.Response
{
    public class ListContentDto : AbstractDashboardDto
    {
        public List<ListEntryDto> Entries { get; set; }

        [JsonProperty("line_variant")]
        public LineVariant LineVariant { get; set; }

        [JsonProperty("leading_element_type")]
        public ElementType LeadingElementType { get; set; }
        
        [JsonProperty("trailing_element_type")]
        public ElementType TrailingElementType { get; set; }
    }
}
