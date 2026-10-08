using System;
using System.Collections.Generic;

namespace Dvelop.Sdk.Home.Dto
{
    [Obsolete("Use Dashboard-App instead")]
    public class FeatureDescriptionDto
    {
        public FeatureDescriptionDto()
        {
            Features = new List<FeatureDto>();
        }
        public List<FeatureDto> Features { get; set; }
    }
}