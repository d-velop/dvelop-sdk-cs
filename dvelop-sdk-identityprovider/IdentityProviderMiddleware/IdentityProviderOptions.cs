﻿using System;
using System.Net.Http;
using Dvelop.Sdk.IdentityProvider.Client;

namespace Dvelop.Sdk.IdentityProvider.Middleware
{
    public class IdentityProviderOptions
    {
        public IdentityProviderOptions()
        {
            BaseAddress = new Uri("http://localhost");
            AllowExternalValidation = false;
        }

        public Uri BaseAddress { get; set; }

        public Func<TenantInformation> TenantInformationCallback { get; set; }
        
        public bool AllowExternalValidation { get; set; }

        public HttpClient HttpClient { get; set; }
    }
}