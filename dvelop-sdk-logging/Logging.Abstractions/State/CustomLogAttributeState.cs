using System.Collections.Generic;
using System.Linq;
using Dvelop.Sdk.Logging.Abstractions.State.Attribute;

namespace Dvelop.Sdk.Logging.Abstractions.State
{
    public class CustomLogAttributeState
    {
        public string Message { get; set; }
        public virtual IEnumerable<CustomLogAttribute> Attributes { get; set; }

        public void Render(ICustomStateRenderer renderer)
        {
            if (!Attributes?.Any() ?? true)
            {
                return;
            }

            foreach (var customLogAttribute in Attributes)
            {
                customLogAttribute.Render(renderer);
            }
        }

        public override string ToString()
        {
            return Message;
        }
    }

}
