namespace Dvelop.Sdk.Logging.Abstractions.Scope
{
    public class TracingLogScope(string traceId, string spanId)
    {
        public string Trace { get; } = traceId;
        public string Span { get; } = spanId;

        public override string ToString()
        {
            return $"TenantLogScope:Trace{Trace}:{Span}";
        }
    }
}
