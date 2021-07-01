namespace Dvelop.Sdk.Logging.Abstractions.Resource
{
    public class ServiceInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Instance { get; set; }

        public ServiceInfo(string name, string version, string instance)
        {
            Name = name;
            Version = version;
            Instance = instance;
        }
    }
}
