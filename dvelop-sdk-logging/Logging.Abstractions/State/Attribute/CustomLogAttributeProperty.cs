using System;

namespace Dvelop.Sdk.Logging.Abstractions.State.Attribute
{
    public class CustomLogAttributeProperty : CustomLogAttribute
    {
        public object Value { get; set; }

        public CustomLogAttributeProperty(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public override void Render(ICustomStateRenderer renderer)
        {
            if (string.IsNullOrWhiteSpace(Key))
            {
                throw new ArgumentException(nameof(Key));
            }

            renderer.Render(this);
        }
    }
}
