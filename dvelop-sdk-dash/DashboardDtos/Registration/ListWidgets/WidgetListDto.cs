namespace Dvelop.Sdk.Dashboard.Dto.Registration.ListWidgets
{
    public class WidgetListDto : WidgetBaseDto
    {
        public const string TYPE = "list";
        
        public override string Type => TYPE;

        public ListQueryDto Query { get; set; }

        public FooterDto Footer { get; set; }
    }
}
