namespace Dvelop.Sdk.Logging.Abstractions.Resource
{
    public class ProcessInfo
    {
        public int Pid { get; set; }

        public ProcessInfo(int pid)
        {
            Pid = pid;
        }
    }
}
