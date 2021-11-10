using System;
using System.Collections.Generic;

namespace Dvelop.Sdk.Dashboard.Dto.Registration.ListWidgets
{
    public class FooterDto : AbstractDashboardDto
    {
        public Uri Url { get; set; }

        public Dictionary<string, string> Label { get; set; }
    }
}
