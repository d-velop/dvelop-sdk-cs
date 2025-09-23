using System.Collections;
using System.Collections.Generic;

namespace Dvelop.Sdk.Logging.Abstractions.Scope
{
    public class TenantLogScope(string tenantId) : IReadOnlyList<KeyValuePair<string, object>>
    {
        private readonly List<KeyValuePair<string, object>> _list = [new("dvelop.tenant.id", tenantId??"0")];
        public string TenantId { get; } = tenantId;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public override string ToString()
        {
            return $"TenantLogScope:TenantId:{TenantId}";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _list.Count;

        public KeyValuePair<string, object> this[int index] => _list[index];
    }
}
