using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dvelop.Sdk.WebApiExtensions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dvelop.Sdk.WebApiExtensions.Filter
{
    public class Dv1HmacSha256SignatureFilter : IAsyncAuthorizationFilter
    {
        private readonly string _secret;

        private readonly ILogger<Dv1HmacSha256SignatureFilter> _log;
        
        public Dv1HmacSha256SignatureFilter(IConfiguration configuration, ILoggerFactory factory)
        {
            _secret = configuration["SIGNATURE_SECRET"];
            _log = factory.CreateLogger<Dv1HmacSha256SignatureFilter>();
        }
        
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            _log.LogInformation("Starting Cloud Event Signature verification.");
            
            var httpContextRequest = context.HttpContext.Request;

            httpContextRequest.EnableBuffering();
            httpContextRequest.Body.Seek(0, SeekOrigin.Begin);

            if (!"DV1-HMAC-SHA256".Equals(httpContextRequest.Headers["x-dv-signature-algorithm"], StringComparison.InvariantCultureIgnoreCase))
            {
                _log.LogInformation("Cloud Event Signature missing.");
                context.Result = new BadRequestResult();
                return;
            }
            
            var signatureHash = await httpContextRequest.CalculateDv1HmacSha256Signature( _secret );
            if (signatureHash == null)
            {
                _log.LogInformation("Cloud Event Signature calculation failed.");
                context.Result = new ForbidResult();
                return;
            }

            string authorization = httpContextRequest.Headers["Authorization"];
            if (authorization.Split(' ').Length != 2 && authorization.Split(' ')[0] != "Bearer")
            {
                _log.LogInformation("Cloud Event Signature checksum missing.");
                context.Result = new ForbidResult();
                return;
            }

            authorization = authorization.Split(' ')[1];
            if (signatureHash != authorization)
            {
                _log.LogInformation($"Cloud Event Signature verification failed (Malformed Bearer Token).");
                context.Result = new ForbidResult();
            }
            
            authorization = authorization.Split(' ')[1];
            if (signatureHash != authorization)
            {
                _log.LogInformation($"Cloud Event Signature verification failed. {signatureHash}!={authorization}");
                context.Result = new ForbidResult();
            }

            var timestamp = httpContextRequest.Headers["x-dv-signature-timestamp"].FirstOrDefault();
            var now = DateTimeOffset.UtcNow;
            if (DateTimeOffset.TryParse(timestamp, out var timeOffset))
            {
                if (timeOffset > now.Add(TimeSpan.FromMinutes(5)) || timeOffset < now.Subtract(TimeSpan.FromMinutes(5)))
                {
                    _log.LogInformation($"Cloud Event Signature verification failed (Timestamp out of valid range: {timeOffset} not in {now} +/- 5min)");
                    context.Result = new ForbidResult();
                }
                else
                {
                    _log.LogInformation($"Cloud Event Signature verification failed (Timestamp in valid range: {timeOffset} -> {now} +/- 5min)");
                }
            }
            else
            {
                _log.LogInformation($"Cloud Event Signature verification failed (No timestamp provided)");
                context.Result = new ForbidResult();
            }
            
            _log.LogInformation("Cloud Event Signature verification ended.");
        }
    }
    
}