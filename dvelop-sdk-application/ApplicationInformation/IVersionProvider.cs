namespace Dvelop.Sdk.ApplicationInformation
{
    public interface IVersionProvider
    {
        SemVer Version { get; }
    }
}