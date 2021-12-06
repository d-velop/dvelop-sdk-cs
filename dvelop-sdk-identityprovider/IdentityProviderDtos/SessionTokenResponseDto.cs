using System;

namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class SessionTokenResponseDto
    {
        public string AuthSessionId { get; set; }

        public DateTime Expire { get; set; }

        public string Sign { get; set; }
    }
}