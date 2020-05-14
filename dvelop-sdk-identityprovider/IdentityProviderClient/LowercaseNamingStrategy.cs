using Newtonsoft.Json.Serialization;

namespace Dvelop.Sdk.IdentityProvider.Client
{
    internal class LowercaseNamingStrategy : NamingStrategy
    {
        protected override string ResolvePropertyName(string name)
        {
            return name.ToLowerInvariant();
        }
    }
}