namespace Dvelop.Sdk.Dashboard.Dto.Registration.IFrameWidget
{
    public class WidgetIFrameDto : WidgetBaseDto
    {
        public const string TYPE = "iframe";

        public override string Type => TYPE;

        public SourceDto Source { get; set; }

        public SizeDto Size { get; set; }
    }
}
