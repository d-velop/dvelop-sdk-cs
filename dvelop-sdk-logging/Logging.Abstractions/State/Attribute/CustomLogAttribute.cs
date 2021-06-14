namespace Dvelop.Sdk.Logging.Abstractions.State.Attribute
{
    public abstract class CustomLogAttribute
    {
        public string Key { get; set; }
        public abstract void Render(ICustomStateRenderer renderer);
    }
}
