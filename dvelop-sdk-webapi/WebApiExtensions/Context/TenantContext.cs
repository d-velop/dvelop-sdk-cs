using System;
using Dvelop.Sdk.BaseInterfaces;
using Microsoft.AspNetCore.Http;

namespace Dvelop.Sdk.WebApiExtensions.Context
{
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _context;

        private static readonly string TenantIdKey = $"{nameof(ITenantContext)}.{nameof(TenantId)}";
        private static readonly string SystemBaseUriKey = $"{nameof(ITenantContext)}.{nameof(SystemBaseUri)}";

        public TenantContext(IHttpContextAccessor context)
        {
            _context = context;
        }

        public string TenantId
        {
            get => _context.HttpContext.Items[TenantIdKey] as string;
            set => _context.HttpContext.Items[TenantIdKey] = value;
        }
        
        public Uri SystemBaseUri
        {
            get => _context.HttpContext.Items[SystemBaseUriKey] as Uri;
            set => _context.HttpContext.Items[SystemBaseUriKey] = value;
        }
    }
}
