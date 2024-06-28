namespace Dvelop.Sdk.Logging.Abstractions.Scope
{
    public class TenantLogScope
    {
        public string TenantId { get; }

        public TenantLogScope(string tenantId)
        {
            TenantId = tenantId;
        }
        
        public override string ToString()
        {
            return $"TenantLogScope:TenantId:{TenantId}";
        }
    }
}
