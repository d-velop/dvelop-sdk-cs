using System;

namespace Dvelop.Sdk.BaseInterfaces
{
    public interface ITenantContext
    {
        string TenantId { get; }
        Uri SystemBaseUri { get; } 
    }
}