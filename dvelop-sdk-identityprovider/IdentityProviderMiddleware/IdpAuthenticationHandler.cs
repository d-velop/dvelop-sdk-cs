using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dvelop.Sdk.IdentityProvider.Middleware
{
    public class IdpAuthenticationHandler(
        IOptionsMonitor<CustomAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<CustomAuthenticationOptions>(options, logger, encoder)
    {
        
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var isIdpAuthenticated = Context.User.Identity?.AuthenticationType?.Equals("d.velop.IdentityProvider") ?? false;
            return Task.FromResult(isIdpAuthenticated
                ? AuthenticateResult.Success(new AuthenticationTicket(Context.User, new AuthenticationProperties(), Scheme.Name))
                : AuthenticateResult.NoResult());
        }
    }
}