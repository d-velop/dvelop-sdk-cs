using System.Drawing;

namespace Dvelop.Sdk.Dashboard.Dto.Registration.ShortcutWidgets
{
    public class WidgetShortcutDto : WidgetBaseDto
    {
        public const string TYPE = "shortcut";
        
        public override string Type => TYPE;

        public ShortcutQueryDto Query { get; set; }


        public Color Test { get; set; }
    }
}
