using System;
using Dvelop.Sdk.Dashboard.Dto.Registration.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dvelop.Sdk.Dashboard.Dto.Registration.ListWidgets
{
    public class QueryContentDto : AbstractDashboardDto
    {
        public Uri Url { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public NavigationType Navigation { get; set; }
    }
}