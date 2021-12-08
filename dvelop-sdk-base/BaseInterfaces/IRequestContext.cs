namespace Dvelop.Sdk.BaseInterfaces
{
    public interface IRequestContext
    {
        string DvRequestId { get; }
        string W3CTraceId { get; }
    }
}