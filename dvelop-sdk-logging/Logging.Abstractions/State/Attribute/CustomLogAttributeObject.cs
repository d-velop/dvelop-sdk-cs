using System;
using System.Collections.Generic;
using System.Linq;

namespace Dvelop.Sdk.Logging.Abstractions.State.Attribute
{
    public class CustomLogAttributeObject : CustomLogAttribute
    {
        public IEnumerable<CustomLogAttribute> Attributes { get; set; }

        public CustomLogAttributeObject(string key, IEnumerable<CustomLogAttribute> attributes)
        {
            Key = key;
            Attributes = attributes;
        }

        public override void Render(ICustomStateRenderer renderer)
        {
            if (string.IsNullOrWhiteSpace(Key))
            {
                throw new ArgumentException(nameof(Key));
            }

            if (!Attributes?.Any() ?? true)
            {
                throw new ArgumentNullException(nameof(Attributes));
            }

            renderer.Render(this);
        }
    }
}
