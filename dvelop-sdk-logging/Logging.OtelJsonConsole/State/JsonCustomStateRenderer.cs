using System;
using System.Collections.Generic;
using System.Text.Json;
using Dvelop.Sdk.Logging.Abstractions.State;
using Dvelop.Sdk.Logging.Abstractions.State.Attribute;
using Dvelop.Sdk.Logging.OtelJsonConsole.Utils;

namespace Dvelop.Sdk.Logging.OtelJsonConsole.State
{
    internal class JsonCustomStateRenderer : ICustomStateRenderer
    {
        private readonly Utf8JsonWriter _writer;

        public JsonCustomStateRenderer(Utf8JsonWriter writer)
        {
            _writer = writer;
        }

        public void Render(CustomLogAttribute customLogAttribute)
        {
            if (customLogAttribute == null)
            {
                throw new ArgumentNullException(nameof(customLogAttribute));
            }

            if (customLogAttribute is CustomLogAttributeProperty customLogAttributeProperty)
            {
                _writer.WriteItem(new KeyValuePair<string, object>(customLogAttributeProperty.Key, customLogAttributeProperty.Value));
            }

            if (customLogAttribute is CustomLogAttributeObject customLogAttributeObject)
            {
                _writer.WriteStartObject(customLogAttributeObject.Key);

                foreach (var attribute in customLogAttributeObject.Attributes)
                {
                    Render(attribute);
                }

                _writer.WriteEndObject();
            }
        }
    }
}
