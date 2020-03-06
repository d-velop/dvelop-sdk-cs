using System.Collections.Generic;

namespace Dvelop.Sdk.Home.Dto
{
    public class FeatureDescriptionDto
    {
        public FeatureDescriptionDto()
        {
            Features = new List<FeatureDto>();
        }
        public List<FeatureDto> Features { get; set; }
    }
}