namespace Dvelop.Sdk.Logging.Abstractions.Scope
{
    public class TracingLogScope
    {
        public string Trace { get; }
        public string Span { get; }

        public TracingLogScope(string traceId, string spanId)
        {
            Trace = traceId;
            Span = spanId;
        }
        
        public override string ToString()
        {
            return $"TenantLogScope:Trace{Trace}:{Span}";
        }
    }
}
