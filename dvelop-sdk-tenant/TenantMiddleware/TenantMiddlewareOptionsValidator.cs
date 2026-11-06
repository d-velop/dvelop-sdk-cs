using System;
using Microsoft.Extensions.Options;

namespace Dvelop.Sdk.TenantMiddleware
{
    /// <summary>
    /// Validates the <see cref="TenantMiddlewareOptions"/>. It is registered automatically when
    /// <see cref="TenantMiddlewareExtensions.AddTenantMiddleware"/> is used.
    /// </summary>
    public sealed class TenantMiddlewareOptionsValidator : IValidateOptions<TenantMiddlewareOptions>
    {
        private const string InvalidUriMessage = "Is no valid URI";

        public ValidateOptionsResult Validate(string name, TenantMiddlewareOptions options)
        {
            if (options is null)
                return ValidateOptionsResult.Fail("TenantMiddlewareOptions must not be null.");

            return GetError(options) switch
            {
                ValidationError.None => ValidateOptionsResult.Success,
                ValidationError.OnTenantIdentifiedMissing => ValidateOptionsResult.Fail(
                    $"{nameof(TenantMiddlewareOptions.OnTenantIdentified)} must be set."),
                ValidationError.DefaultSystemBaseUriInvalid => ValidateOptionsResult.Fail(
                    $"{nameof(TenantMiddlewareOptions.DefaultSystemBaseUri)}: {InvalidUriMessage}"),
                _ => throw new InvalidOperationException("Unknown validation error")
            };
        }

        /// <summary>
        /// Throws the same exceptions as the constructors did before the validation was extracted.
        /// </summary>
        internal static void ThrowIfInvalid(TenantMiddlewareOptions tenantMiddlewareOptions)
        {
            if (tenantMiddlewareOptions is null) throw new ArgumentNullException(nameof(tenantMiddlewareOptions));

            switch (GetError(tenantMiddlewareOptions))
            {
                case ValidationError.OnTenantIdentifiedMissing:
                    throw new ArgumentNullException(nameof(tenantMiddlewareOptions.OnTenantIdentified));
                case ValidationError.DefaultSystemBaseUriInvalid:
                    throw new ArgumentException(InvalidUriMessage, nameof(tenantMiddlewareOptions.DefaultSystemBaseUri));
            }
        }

        private static ValidationError GetError(TenantMiddlewareOptions options)
        {
            if (options.OnTenantIdentified is null)
                return ValidationError.OnTenantIdentifiedMissing;
            if (options.DefaultSystemBaseUri is not null &&
                !Uri.IsWellFormedUriString(options.DefaultSystemBaseUri, UriKind.RelativeOrAbsolute))
                return ValidationError.DefaultSystemBaseUriInvalid;
            return ValidationError.None;
        }

        private enum ValidationError
        {
            None,
            OnTenantIdentifiedMissing,
            DefaultSystemBaseUriInvalid
        }
    }
}
