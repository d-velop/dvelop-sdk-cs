using System.Collections.Generic;

namespace Dvelop.Sdk.Logging.Abstractions.Scope
{
    public interface ICustomAttributesLogScope
    {
        Dictionary<string, object> Items { get; }
        string Name { get; }
    }

    public class CustomAttributesLogScope : ICustomAttributesLogScope
    {
        public Dictionary<string, object> Items { get; }
        public string Name { get; }

        public CustomAttributesLogScope(string name, Dictionary<string, object> items)
        {
            Name = name;
            Items = items;
        }
    }
}
