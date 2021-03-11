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

        // ReSharper disable once InconsistentNaming
        private const string IDP_QUERY_NO_ID_DETAIL_LEVEL = "detaillevel=2";
        
        //TODO: evaluate https://docs.microsoft.com/de-de/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        private readonly HttpClient _httpClient;
        private readonly IdentityProviderSessionStore _sessionStore = new IdentityProviderSessionStore();

        private readonly Func<TenantInformation> _tenantInformationCallback;
        private readonly bool _allowExternalValidation;
        private readonly Action<IdentityProviderClientLogLevel, string> _logCallback;
        private readonly string _defaultSystemBaseUri;
        private readonly bool _useMinimizedOnlyIdValidateDetailLevel;
        private readonly bool _allowAppSessions;
        private readonly List<string> _allowedImpersonatedApps;
        private readonly bool _allowImpersonatedUsers;
        

        [Obsolete("Use 'IdentityProviderClientOptions' instead. This constructor may be removed in a later releases")]
        public IdentityProviderClient(HttpClient httpClient, Func<TenantInformation> tenantInformationCallback,
            bool allowExternalValidation = false)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tenantInformationCallback = tenantInformationCallback ?? throw new ArgumentNullException(nameof(tenantInformationCallback));
            _allowExternalValidation = allowExternalValidation;
        }
        
        public IdentityProviderClient(IdentityProviderClientOptions options)
        {
            _tenantInformationCallback = options.TenantInformationCallback;
            _allowExternalValidation = options.AllowExternalValidation;
            _httpClient = options.HttpClient ?? new HttpClient();
            _defaultSystemBaseUri = options.SystemBaseUri.ToString();
            _useMinimizedOnlyIdValidateDetailLevel = options.UseMinimizedOnlyIdValidateDetailLevel;
            _allowAppSessions = options.AllowAppSessions;
            _allowedImpersonatedApps = options.AllowedImpersonatedApps;
            _allowImpersonatedUsers = options.AllowImpersonatedUsers;
            _logCallback = options.LogCallBack;
            _logCallback?.Invoke(IdentityProviderClientLogLevel.Info, "IdentityProviderClient initialized");
        }

        public static string CookieName => COOKIENAME;
        public bool IsExternalValidationAllowed()
        {
            return _allowExternalValidation;
        }
        
        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(string authSessionId)
        {
            var tenantId = "0";
            var systemBaseUri = _defaultSystemBaseUri;
            if (_tenantInformationCallback != null)
            {
                var ti = _tenantInformationCallback();
                tenantId = ti.TenantId;
                systemBaseUri = ti.SystemBaseUri;
            }

            var result = _sessionStore.GetPrincipal(authSessionId + "-" + tenantId);
            if (result == null)
            {
                _logCallback?.Invoke(IdentityProviderClientLogLevel.Debug, "Principal for authSessionId not found in cache");
                return await CreatePrincipalAsync(authSessionId, tenantId, systemBaseUri).ConfigureAwait(false);
            }
            _logCallback?.Invoke(IdentityProviderClientLogLevel.Debug, "Principal for authSessionId found in cache");
            return result;

        }

        public async Task<AuthSessionInfoDto> GetAuthSessionIdFromApiKey(string apiKey)
        {
            var systemBaseUri = _defaultSystemBaseUri;
            if (_tenantInformationCallback != null)
            {
                var ti = _tenantInformationCallback();
                systemBaseUri = ti.SystemBaseUri;
            }
            var loginUri = systemBaseUri + IDPBASE + IDP_LOGIN;
            
            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, loginUri)
                {
                    Headers =
                    {
                        {"Authorization", $"Bearer {apiKey}"},
                        {"Accept", "application/json"},
                    }
                }
            ).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK ||
                !response.Content.Headers.ContentType.MediaType.StartsWith("application/json",
                    StringComparison.OrdinalIgnoreCase))
            {
                _logCallback?.Invoke(IdentityProviderClientLogLevel.Debug, "Unable to create AuthSessionId from API_KEY");
                return null;
            }
            var authSessionInfoDto = JsonConvert.DeserializeObject<AuthSessionInfoDto>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            _logCallback?.Invoke(IdentityProviderClientLogLevel.Debug, "Successfully to created AuthSessionId from API_KEY");
            return authSessionInfoDto;
        }

        public Uri GetLoginUri(string redirect)
        {
            return new Uri("/" + IDPBASE + string.Format(IDP_LOGIN, HttpUtility.UrlEncode(redirect)), UriKind.Relative);
        }


        private ClaimsPrincipal UserDtoToClaimsPrincipal(string authSessionId,UserDto userDto,string tenantId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.SerialNumber, userDto.Id),
                new Claim(ClaimTypes.Surname, userDto.Name?.FamilyName ?? string.Empty),
                new Claim(ClaimTypes.GivenName, userDto.Name?.GivenName ?? string.Empty),
                new Claim("DisplayName", userDto.DisplayName ?? string.Empty),
                new Claim(ClaimTypes.Name, userDto.UserName ?? string.Empty)
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
            claims.Add(new Claim("com.dvelop.tenantid", !String.IsNullOrWhiteSpace(tenantId)?tenantId:"0"));
            _logCallback?.Invoke(IdentityProviderClientLogLevel.Debug, $"ClaimsPrincipal for user with id={userDto.Id} created.");
            return new ClaimsPrincipal(new ClaimsIdentity(claims, PROVIDERNAME));
        }

        private async Task<ClaimsPrincipal> CreatePrincipalAsync(string authSessionId, string tenantId, string systemBaseUri)
        {
            var validateUri = systemBaseUri + IDPBASE + IDP_VALIDATE;
            if (_allowExternalValidation)
            {
                validateUri += IDP_QUERY_EXTERNAL;
            }

            if (_useMinimizedOnlyIdValidateDetailLevel)
            {
                if (_allowExternalValidation)
                {
                    validateUri += "&" + IDP_QUERY_NO_ID_DETAIL_LEVEL;
                }
                else
                {
                    validateUri += "?" + IDP_QUERY_NO_ID_DETAIL_LEVEL;
                }
            }

            _logCallback?.Invoke(IdentityProviderClientLogLevel.Debug, $"Sending validate request to d.ecs identity provider ({validateUri})...");
            
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, validateUri)
                {
                    Headers =
                    {
                        {"Authorization", $"Bearer {authSessionId}"},
                        {"Accept", "application/json"},
                    }
                }).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                _logCallback?.Invoke(IdentityProviderClientLogLevel.Warning, "Probably a timeout occurred while validating the session");
                throw;
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException!=null && ae.InnerException.GetType()==typeof(TaskCanceledException))
                    _logCallback?.Invoke(IdentityProviderClientLogLevel.Warning, "Probably a timeout occurred while validating the session");
                throw;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logCallback?.Invoke(IdentityProviderClientLogLevel.Debug, "d.ecs identity provider response was UNAUTHORIZED");
                return null;
            }

            if (response.StatusCode != HttpStatusCode.OK || response.Content.Headers.ContentType==null ||
                !response.Content.Headers.ContentType.MediaType.StartsWith("application/json",
                    StringComparison.OrdinalIgnoreCase))
            {
                _logCallback?.Invoke(IdentityProviderClientLogLevel.Debug, $"d.ecs identity provider response was {response.StatusCode} and/or not application/json");
                return new ClaimsPrincipal();
            }

            var userDto = JsonConvert.DeserializeObject<UserDto>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

            if (userDto == null)
            {
                _logCallback?.Invoke(IdentityProviderClientLogLevel.Error, "d.ecs identity provider response could not be deserialized");
                return null;
            }

            //Überprüfen ob App-Session
            if (!_allowAppSessions && userDto.Id.EndsWith(IdpConst.APP_ID_SUFFIX))
            {
                _logCallback?.Invoke(IdentityProviderClientLogLevel.Info, "This is an app-session and the current settings forbid app-sessions");
                return null;
            }
            //Überprüfen ob Impersonated-Session
            if (!_allowImpersonatedUsers || _allowedImpersonatedApps!=null && _allowedImpersonatedApps.Any())
            {
                if (response.Headers.TryGetValues("x-dv-impersonated", out var values))
                {
                    if (values.Any())
                    {
                        if (_allowedImpersonatedApps==null || !_allowedImpersonatedApps.Contains(values.First().ToLower()))
                        {
                            _logCallback?.Invoke(IdentityProviderClientLogLevel.Info, "This is an impersonated session and the current settings forbid this");
                            return null;
                        }
                    }
                }
            }

            var principal = UserDtoToClaimsPrincipal(authSessionId, userDto, tenantId);
            var maxAge = response.Headers.CacheControl.MaxAge;
            if (maxAge == null)
            {
                _logCallback?.Invoke(IdentityProviderClientLogLevel.Info, "Max-Age response header not present");
                return null;
            }
            _sessionStore.SetPrincipal(authSessionId + "-" + tenantId, DateTime.Now.AddSeconds(maxAge.Value.TotalSeconds), principal);

            return principal;
        }
    }
}