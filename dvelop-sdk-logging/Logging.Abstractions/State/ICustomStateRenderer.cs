using Dvelop.Sdk.Logging.Abstractions.State.Attribute;

namespace Dvelop.Sdk.Logging.Abstractions.State
{
    public interface ICustomStateRenderer
    {
        void Render(CustomLogAttribute customLogAttribute);
    }
}
