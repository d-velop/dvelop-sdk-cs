using System;

namespace Dvelop.Sdk.Home.Dto
{
    
    public class FeatureDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
        public string IconUri { get; set; }
        public FeatureBadgeDto Badge { get; set; }
    }
}