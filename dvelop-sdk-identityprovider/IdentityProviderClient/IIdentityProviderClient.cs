using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dvelop.Sdk.IdentityProvider.Dto;

namespace Dvelop.Sdk.IdentityProvider.Client
{
    public interface IIdentityProviderClient
    {
        bool IsExternalValidationAllowed();
        Task<ClaimsPrincipal> GetClaimsPrincipalAsync(string authSessionId);
        Task<bool> RequestAppSession(AppSessionRequestDto requestDto);
        Task<AuthSessionInfoDto> GetAuthSessionIdFromApiKey(string apiKey);
        Uri GetLoginUri(string redirect);
    }
}