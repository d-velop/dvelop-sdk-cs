using System.Collections.Generic;

namespace Dvelop.Sdk.Dashboard.Dto.Registration.Base
{
    public class PermissionDto : AbstractDashboardDto
    {
        public List<StringValueDto> Groups { get; set; }
    }
}