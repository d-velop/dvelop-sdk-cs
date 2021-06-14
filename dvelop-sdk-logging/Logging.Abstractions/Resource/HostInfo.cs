namespace Dvelop.Sdk.Logging.Abstractions.Resource
{
    public class HostInfo
    {
        public string Hostname { get; set; }

        public HostInfo(string hostname)
        {
            Hostname = hostname;
        }
    }
}
