using System;
using System.Net.Http;
using Dvelop.Sdk.IdentityProvider.Client;

namespace Dvelop.Sdk.IdentityProvider.Middleware
{
    public class IdentityProviderOptions
    {
        public IdentityProviderOptions()
        {
            TriggerAuthentication = false;
            AllowExternalValidation = false;
        }

        public bool TriggerAuthentication { get; set; }

        public Func<TenantInformation> TenantInformationCallback { get; set; }
        
        public bool AllowExternalValidation { get; set; }

        public HttpClient HttpClient { get; set; }
    }
}