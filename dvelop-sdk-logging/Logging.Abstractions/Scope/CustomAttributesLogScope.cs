using System.Collections.Generic;
using System.Linq;

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

        public override string ToString()
        {
            return $"CustomAttributesLogScope:{Name} => Items: {Items.Select(i=>i.Key + ":" + i.Value)+";"}";
        }
    }
}
