using Dvelop.Sdk.Dashboard.Dto.Registration.Base;
using Newtonsoft.Json;

namespace Dvelop.Sdk.Dashboard.Dto.Response.Types
{
    public class ListEntryDto : AbstractDashboardDto
    {
        public TextDto Title { get; set; }

        public TextDto Subtitle { get; set; }

        public FontMode FontMode { get; set; }

        [JsonProperty("lead_element")]
        public ElementDto LeadElement { get; set; }

        [JsonProperty("trail_element")]
        public ElementDto TrailElement { get; set; }

        public TargetDto Target { get; set; }
    }
}
