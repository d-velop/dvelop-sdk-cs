using System;
using Dvelop.Sdk.BaseInterfaces;
using Microsoft.AspNetCore.Http;

namespace Dvelop.Sdk.WebApiExtensions.Context
{
    public class TenantContext(IHttpContextAccessor context) : ITenantContext
    {
        private static readonly string TenantIdKey = $"{nameof(ITenantContext)}.{nameof(TenantId)}";
        private static readonly string SystemBaseUriKey = $"{nameof(ITenantContext)}.{nameof(SystemBaseUri)}";

        public string TenantId
        {
            get => context.HttpContext?.Items[TenantIdKey] as string;
            set
            {
                if (context.HttpContext != null) context.HttpContext.Items[TenantIdKey] = value;
            }
        }

        public Uri SystemBaseUri
        {
            get => context.HttpContext?.Items[SystemBaseUriKey] as Uri;
            set
            {
                if (context.HttpContext != null) context.HttpContext.Items[SystemBaseUriKey] = value;
            }
        }
    }
}
