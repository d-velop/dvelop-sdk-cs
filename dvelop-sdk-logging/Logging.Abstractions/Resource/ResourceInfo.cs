namespace Dvelop.Sdk.Logging.Abstractions.Resource
{
    public class ResourceInfo
    {
        public ServiceInfo Service { get; set; }
        public HostInfo Host { get; set; }
        public ProcessInfo Process { get; set; }

        public ResourceInfo(ServiceInfo service, HostInfo host, ProcessInfo process)
        {
            Service = service;
            Host = host;
            Process = process;
        }
    }
}
