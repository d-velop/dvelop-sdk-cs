using System.Collections.Generic;

namespace Dvelop.Sdk.Config.Dto
{
    public class PredefinedHeadlineDto
    {
        public string Group { get; set; }
        public List<MenuItemDto> MenuItems { get; set; }

        public PredefinedHeadlineDto()
        {
            MenuItems = new List<MenuItemDto>();
        }
    }
}