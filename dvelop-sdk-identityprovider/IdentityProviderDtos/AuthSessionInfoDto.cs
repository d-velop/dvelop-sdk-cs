using System;

namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class AuthSessionInfoDto
    {
        public string AuthSessionId { get; set; }
        public DateTime Expire { get; set; }
    }
}
