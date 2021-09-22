using System.Collections.Generic;
using Dvelop.Sdk.Logging.Abstractions.Scope;

namespace Dvelop.Sdk.Logging.OtelJsonConsole.Scope
{
    internal class ScopeInfo
    {
        public bool Visible { get; set; }
        public IList<ICustomAttributesLogScope> CustomAttributes { get; set; }
        public IList<object> Scopes { get; set; }
        public TenantLogScope TenantLogScope { get; set; }
        public TracingLogScope TracingLogScope { get; set; }
    }
}
