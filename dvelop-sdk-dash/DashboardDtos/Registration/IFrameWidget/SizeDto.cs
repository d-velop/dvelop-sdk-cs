using Newtonsoft.Json;

namespace Dvelop.Sdk.Dashboard.Dto.Registration.IFrameWidget
{
    public class SizeDto
    {
        [JsonProperty("min_w")]
        public int MinWidth { get; set; }
        
        [JsonProperty("min_h")]
        public int MinHeight { get; set; }
    }
}
