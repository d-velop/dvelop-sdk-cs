using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dvelop.Sdk.TenantMiddleware
{
    public class TenantMiddleware
    {
        private readonly  TenantMiddlewareOptions _tenantMiddlewareOptions;
        private readonly RequestDelegate _next;

        // ReSharper disable once InconsistentNaming
        internal const string SYSTEM_BASE_URI_HEADER = "x-dv-baseuri";
        // ReSharper disable once InconsistentNaming
        internal const string TENANT_ID_HEADER = "x-dv-tenant-id";
        // ReSharper disable once InconsistentNaming
        internal const string SIGNATURE_HEADER = "x-dv-sig-1";

        public TenantMiddleware(RequestDelegate next, TenantMiddlewareOptions tenantMiddlewareOptions) 
        {
            if (tenantMiddlewareOptions == null) throw new ArgumentNullException(nameof(tenantMiddlewareOptions));
            if (tenantMiddlewareOptions.OnTenantIdentified == null)
                throw new ArgumentNullException(nameof(tenantMiddlewareOptions.OnTenantIdentified));
            if (tenantMiddlewareOptions.DefaultSystemBaseUri != null &&
                !Uri.IsWellFormedUriString(tenantMiddlewareOptions.DefaultSystemBaseUri, UriKind.RelativeOrAbsolute))
                throw new ArgumentException("Is no valid URI", nameof(tenantMiddlewareOptions.DefaultSystemBaseUri));

            _tenantMiddlewareOptions = tenantMiddlewareOptions;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var systemBaseUriFromHeader = context.Request.Headers[SYSTEM_BASE_URI_HEADER];
            var tenantIdFromHeader = context.Request.Headers[TENANT_ID_HEADER];
            var base64Signature = context.Request.Headers[SIGNATURE_HEADER];

            var status = Invoke(_tenantMiddlewareOptions, systemBaseUriFromHeader, tenantIdFromHeader, base64Signature);
            if (status != 0)
            {
                context.Response.StatusCode = (int)status;
                return;
            }

            await _next(context);
        }

        internal static HttpStatusCode Invoke(TenantMiddlewareOptions tenantMiddlewareOptions, string systemBaseUriFromHeader,
            string tenantIdFromHeader, string base64Signature)
        {
            if (systemBaseUriFromHeader != null || tenantIdFromHeader != null)
            {
                if (base64Signature == null)
                {
                    tenantMiddlewareOptions.LogCallback?.Invoke(TenantMiddlewareLogLevel.Debug, "Signature is missing in request header");
                    return HttpStatusCode.Forbidden;
                }
                if (tenantMiddlewareOptions.SignatureSecretKey == null)
                {
                    tenantMiddlewareOptions.LogCallback?.Invoke(TenantMiddlewareLogLevel.Error, "SignatureSecretKey is missing in tenantMiddlewareOptions");
                    return HttpStatusCode.InternalServerError;
                }

                if (tenantMiddlewareOptions.IgnoreSignature)
                {
                    tenantMiddlewareOptions.LogCallback?.Invoke(TenantMiddlewareLogLevel.Error, "Signature is ignored, don't use this in production environment!");
                }
                else
                {
                    var encoding = new ASCIIEncoding();
                    var data = systemBaseUriFromHeader + tenantIdFromHeader;
                    tenantMiddlewareOptions.LogCallback?.Invoke(TenantMiddlewareLogLevel.Debug, $"Signature will be calculated using '{data}'");
                    var messageBytes = encoding.GetBytes(data);
                    try
                    {
                        var signature = Convert.FromBase64String(base64Signature);
                        if (!SignatureIsValid(messageBytes, signature, tenantMiddlewareOptions.SignatureSecretKey))
                        {
                            tenantMiddlewareOptions.LogCallback?.Invoke(TenantMiddlewareLogLevel.Debug,
                                "Signature does not match");
                            return HttpStatusCode.Forbidden;
                        }
                        tenantMiddlewareOptions.LogCallback?.Invoke(TenantMiddlewareLogLevel.Debug,
                            "Signature matches!");
                    }
                    catch (FormatException)
                    {
                        tenantMiddlewareOptions.LogCallback?.Invoke(TenantMiddlewareLogLevel.Error,
                            "Signature is in wrong format");
                        return HttpStatusCode.Forbidden;
                    }
                    catch (Exception e)
                    {
                        tenantMiddlewareOptions.LogCallback?.Invoke(TenantMiddlewareLogLevel.Error,
                            $"Exception while checking signature. Exception={e.Message}");
                        return HttpStatusCode.Forbidden;
                    }
                }
            }
            tenantMiddlewareOptions.OnTenantIdentified(tenantIdFromHeader ?? tenantMiddlewareOptions.DefaultTenantId,
                systemBaseUriFromHeader ?? tenantMiddlewareOptions.DefaultSystemBaseUri);
            tenantMiddlewareOptions.LogCallback?.Invoke(TenantMiddlewareLogLevel.Debug,
                "Tenant identified!");
            return 0;
        }

        private static bool SignatureIsValid(byte[] message, byte[] signature, byte[] sigKey)
        {
            using (var mac = new HMACSHA256(sigKey))
            {
                var expectedSignature = mac.ComputeHash(message);
                return AreEqualConstantTime(expectedSignature, signature);
            }
        }

        /// <summary>
        /// Compares two byte arrays for equality without leaking timing information. cf. https://codahale.com/a-lesson-in-timing-attacks/
        /// </summary>
        /// <param name="internalValue"></param>
        /// <param name="externalValue"></param>
        /// <returns></returns>
        // Preventing function runtime changes at compile time or JIT compilations due to optimizations
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static bool AreEqualConstantTime(byte[] internalValue, byte[] externalValue)
        {
            // A missing internally calculated value must never happen.
            // These two checks depend only on internal parameters and may therefore alter the function's runtime.
            if (null == internalValue)
                throw new ArgumentNullException(nameof(internalValue));

            if (0 == internalValue.Length)
                throw new ArgumentException("invalid internal value");

            // Providing the same variable for both arrays must never happen.
            var areTheSameVariables = internalValue == externalValue;
            if (areTheSameVariables)
                throw new ArgumentException("both parameters are the same variable");

            var tempResult = 0;

            // A missing external value must not cause a different function runtime, so we provide a legit value to compare the internal value to.
            // The internal comparison state is set to never give a positive answer.
            // The array must be cloned every time regardless of any validation results to have a constant function runtime.
            var tempValue = (byte[])internalValue.Clone();
            if (null == externalValue)
            {
                tempResult = 1;
                externalValue = tempValue;
            }

            // An external value with a different length must not cause a different function runtime, so we provide a legit value to compare the internal value to.
            // The internal comparison state is set to never give a positive answer.
            // Do not combine both validations as it will cause a different function runtime when using a logical 'or'.
            if (internalValue.Length != externalValue.Length)
            {
                tempResult = 1;
                externalValue = internalValue;
            }

            // Comparing every value, regardless of the equality or inequality of former comparisons.
            for (var index = 0; index < internalValue.Length; ++index)
                // Xor'ing two equal values results in a zero, so the temporary result variable's value does not change.
                // In case of a mismatch the value will not be zero and the internal comparion state will be changed.
                // It is impossible to get rid of this difference by providing carefully crafted chechsums once the internal comparison state has changed.
                tempResult |= (internalValue[index] ^ externalValue[index]);

            return 0 == tempResult;
        }
    }
}