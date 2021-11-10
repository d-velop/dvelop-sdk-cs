using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Dvelop.Sdk.Dashboard.Dto.Response.Types
{
    public class ElementDto : AbstractDashboardDto
    {
        [JsonProperty("material_icon")]
        public string MaterialIcon { get; set; }
        
        [JsonProperty("icon_color")] 
        public Color IconColor { get; set; }
        
        [JsonProperty("image_url")] 
        public Uri ImageUrl { get; set; }
        
        [JsonProperty("image_size")] 
        public ImageSize ImageSize { get; set; }
        
        public string Caption { get; set; }
        
        [JsonProperty("caption_color")] 
        public Color CaptionColor { get; set; }
    }
}
