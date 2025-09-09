using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dvelop.Sdk.Logging.Abstractions.Scope
{
    public interface ICustomAttributesLogScope: IReadOnlyList<KeyValuePair<string, object>>
    {
        Dictionary<string, object> Items { get; }
        string Name { get; }
    }

    public class CustomAttributesLogScope(string name, Dictionary<string, object> items) : ICustomAttributesLogScope
    {
        public Dictionary<string, object> Items { get; } = items;
        public string Name { get; } = name;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public override string ToString()
        {
            return $"CustomAttributesLogScope:{Name} => Items: {string.Join(", ", Items.Select(i => $"'{i.Key}:{i.Value}'"))}";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => Items.Count;

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                (string key, object value) = Items.ToList()[index];
                return new KeyValuePair<string, object>($"{name}.{key}", value);
            }
        }
    }
}
