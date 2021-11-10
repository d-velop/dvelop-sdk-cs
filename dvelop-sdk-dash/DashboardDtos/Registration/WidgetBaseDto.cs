using System.Collections.Generic;
using Dvelop.Sdk.Dashboard.Dto.Registration.Base;

namespace Dvelop.Sdk.Dashboard.Dto.Registration
{
    public abstract class WidgetBaseDto : AbstractDashboardDto
    {
        public abstract string Type { get; }

        public Dictionary<string, string> Title { get; set; }
        
        public Dictionary<string, string> Subtitle { get; set; }

        public IconDto Icon { get; set; }

        public TargetDto Target { get; set; }
    }
}
