using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Dvelop.Sdk.IdentityProvider.Dto;
using Newtonsoft.Json;

namespace Dvelop.Sdk.IdentityProvider.Client
{
    public class IdentityProviderClient
    {
        // ReSharper disable once InconsistentNaming
        private const string COOKIENAME = "AuthSessionId";

        // ReSharper disable once InconsistentNaming
        private const string PROVIDERNAME = "d.velop.IdentityProvider";
        
        // ReSharper disable once InconsistentNaming
        private const string IDPBASE = "identityprovider";

        // ReSharper disable once InconsistentNaming
        private const string IDP_VALIDATE = "/validate";

        // ReSharper disable once InconsistentNaming
        private const string IDP_QUERY_EXTERNAL = "?allowExternalValidation=true";

        // ReSharper disable once InconsistentNaming
        private const string IDP_LOGIN = "/login?redirect={0}";

        
        //TODO: evaluate https://docs.microsoft.com/de-de/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        private readonly HttpClient _httpClient;
        private readonly IdentityProviderSessionStore _sessionStore = new IdentityProviderSessionStore();

        private readonly Func<TenantInformation> _tenantInformationCallback;
        private readonly bool _allowExternalValidation;

        public IdentityProviderClient(HttpClient httpClient, Func<TenantInformation> tenantInformationCallback,
            bool allowExternalValidation = false)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tenantInformationCallback = tenantInformationCallback ?? throw new ArgumentNullException(nameof(tenantInformationCallback));
            _allowExternalValidation = allowExternalValidation;
        }

        public static string CookieName => COOKIENAME;
        public bool IsExternalValidationAllowed()
        {
            return _allowExternalValidation;
        }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(string authSessionId)
        {
            var tenantInformation = _tenantInformationCallback();
            var tenantId = tenantInformation.TenantId;
            var systemBaseUri = tenantInformation.SystemBaseUri;

            return _sessionStore.GetPrincipal(authSessionId + "-" + tenantId) ??
                   await CreatePrincipalAsync(authSessionId, tenantId, systemBaseUri).ConfigureAwait(false);
        }

        public async Task<AuthSessionInfoDto> GetAuthSessionIdFromApiKey(string apiKey)
        {

            var tenantInformation = _tenantInformationCallback();
            var systemBaseUri = tenantInformation.SystemBaseUri;
            
            var loginUri = systemBaseUri + IDPBASE + IDP_LOGIN;

            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, loginUri)
                {
                    Headers =
                    {
                        {"Authorization", $"Bearer {apiKey}"},
                        {"Accept", "application/json"}
                    }
                }
            );

            if (response.StatusCode != HttpStatusCode.OK ||
                !response.Content.Headers.ContentType.MediaType.StartsWith("application/json",
                    StringComparison.OrdinalIgnoreCase)) return null;
            var authSessionInfoDto =
                JsonConvert.DeserializeObject<AuthSessionInfoDto>(await response.Content.ReadAsStringAsync());
            return authSessionInfoDto;
        }

        public Uri GetLoginUri(string redirect)
        {
            return new Uri("/" + IDPBASE + string.Format(IDP_LOGIN, HttpUtility.UrlEncode(redirect)), UriKind.Relative);
        }


        private static ClaimsPrincipal UserDtoToClaimsPrincipal(string authSessionId, UserDto userDto, string tenantId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.SerialNumber, userDto.Id),
                new Claim(ClaimTypes.Surname, userDto.Name?.FamilyName ?? string.Empty),
                new Claim(ClaimTypes.GivenName, userDto.Name?.GivenName ?? string.Empty),
                new Claim("DisplayName", userDto.DisplayName ?? string.Empty),
                new Claim(ClaimTypes.Name, userDto.UserName)
            };
            var email = userDto.Emails?.FirstOrDefault();
            if (email != null) claims.Add(new Claim(ClaimTypes.Email, email.Value));
            var phone = userDto.PhoneNumbers?.FirstOrDefault();
            if (phone != null) claims.Add(new Claim("Phone", phone.Value));
            var photo = userDto.Photos?.FirstOrDefault();
            if (photo != null) claims.Add(new Claim("PhotoUri", photo.Value));
            if (userDto.Groups != null)
                claims.AddRange(userDto.Groups.Select(valueDto => new Claim(ClaimTypes.Role, valueDto.Value)));

            //Legacy Claims
            claims.Add(new Claim("com.dvelop.user.id", userDto.Id));
            claims.Add(new Claim("com.dvelop.user.is_admin",
                (userDto.Groups?.FirstOrDefault(g => g.Value == IdpConst.BUILT_IN_ADMIN_GROUP) != null).ToString()));
            claims.Add(new Claim("com.dvelop.bearer", authSessionId));
            claims.Add(new Claim("com.dvelop.tenantid", !String.IsNullOrWhiteSpace(tenantId) ? tenantId : "0"));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, PROVIDERNAME));
        }

        private async Task<ClaimsPrincipal> CreatePrincipalAsync(string authSessionId, string tenantId,
            string systemBaseUri)
        {
            var validateUri = systemBaseUri + IDPBASE + IDP_VALIDATE;
            if (_allowExternalValidation)
            {
                validateUri += IDP_QUERY_EXTERNAL;
            }

            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, validateUri)
                {
                    Headers =
                    {
                        {"Authorization", $"Bearer {authSessionId}"},
                        {"Accept", "application/json"},
                    }
                }
            );

            if (response.StatusCode == HttpStatusCode.Unauthorized) return null;

            if (response.StatusCode != HttpStatusCode.OK ||
                !response.Content.Headers.ContentType.MediaType.StartsWith("application/json",
                    StringComparison.OrdinalIgnoreCase)) return new ClaimsPrincipal();

            var userDto = JsonConvert.DeserializeObject<UserDto>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            var principal = UserDtoToClaimsPrincipal(authSessionId, userDto, tenantId);
            var maxAge = response.Headers.CacheControl.MaxAge;
            if (maxAge == null) return null;
            _sessionStore.SetPrincipal(authSessionId + "-" + tenantId,
                DateTime.Now.AddSeconds(maxAge.Value.TotalSeconds), principal);

            return principal;
        }
    }
}