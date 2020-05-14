using System;

namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class AppSessionRequestDto
    {
        public string AppName { get; set; }
        public Uri Callback { get; set; }
        public string RequestId { get; set; }
    }
}