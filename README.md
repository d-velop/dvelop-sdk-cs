# d.velop cloud SDK for .NET

This is the official SDK to build Apps for [d.velop cloud](https://www.d-velop.de/cloud/) using the C# programming language.

> [!WARNING]
> **Deprecation of the Home-App:** The d.velop Home-App is deprecated and will be replaced by the Dashboard-App.
> The DTOs in `Dvelop.Sdk.Home.Dto` (`FeatureDto`, `FeatureDescriptionDto`, `FeatureBadgeDto`) are marked as `[Obsolete]`
> and will be removed in a future release. The Dashboard-App has a different structure, so there is no one-to-one replacement for these types.

The project has alpha status. **So for now expect things to change.**

## Packages

| Package                             | Purpose                                                                                               |
|-------------------------------------|-------------------------------------------------------------------------------------------------------|
| `Dvelop.Sdk`                        | All-in-one package that references all the packages below                                             |
| `Dvelop.Sdk.TenantMiddleware`       | Tenant identification and validation of the `x-dv-sig-1` signature                                    |
| `Dvelop.Sdk.IdentityProvider.*`     | Auth session validation, user/group DTOs, IdentityProvider middleware and client                      |
| `Dvelop.Sdk.SigningAlgorithms`      | HMAC-SHA256 and DV1-HMAC-SHA256 signing primitives                                                    |
| `Dvelop.Sdk.WebApiExtensions`       | Request/tenant/W3C-tracing logging middleware, request signing                                        |
| `Dvelop.Sdk.Logging.*`              | OTEL JSON console logging                                                                             |
| `Dvelop.Sdk.HttpClientExtensions`   | `HttpClient` helpers                                                                                  |
| DTO packages                        | Base, CloudCenter, Config, Dashboard and (deprecated) Home DTOs                                       |
| `Dvelop.Sdk.ApplicationInformation` | `IVersionProvider` and `SemVer` (see [below](#reading-the-application-version-with-iversionprovider)) |

## Usage

Include the d.velop sdk packages as dependencies in your .NET 10 project file (`myproject.csproj`) and restore them with `dotnet restore` via commandline or from within your IDE.

Example:
```xml
<ItemGroup>
    <PackageReference Include="Dvelop.Sdk.TenantMiddleware" Version="0.2.0.*" />
    <PackageReference Include="Dvelop.Sdk.IdentityProvider.Middleware" Version="0.2.0.*" />
</ItemGroup>
```

or the all-in-one dependency:

```xml
<ItemGroup>
    <PackageReference Include="Dvelop.Sdk" Version="0.2.0.*" />
</ItemGroup>
```

The most recent version can be installed from [nuget.org](https://www.nuget.org/packages/Dvelop.Sdk)

A running Application, which uses this SDK can be found at [github.com/d-velop/dvelop-app-template-cs](https://github.com/d-velop/dvelop-app-template-cs)

### Reading the application version with `IVersionProvider`

`IVersionProvider` exposes the version of the running application as a `SemVer` (`Major`, `Minor`, `Patch`, `Qualifier`).
`VersionProvider` is the default implementation. It reads the `AssemblyInformationalVersion` of the assembly you pass to it
and parses it, e.g. `1.2.3-beta` or `1.2.3+build.5`. If the version has no qualifier, `Qualifier` is an empty string.

Because the provider lives in the SDK library, you have to tell it which assembly to read, otherwise it would report the SDK's own version.
Register it in your dependency injection container with the assembly of your application:

```csharp
using Dvelop.Sdk.ApplicationInformation;

builder.Services.AddSingleton<IVersionProvider>(new VersionProvider(typeof(Program).Assembly));
```

and inject it where you need the version:

```csharp
app.MapGet("/version", (IVersionProvider versionProvider) =>
{
    SemVer version = versionProvider.Version;
    return Results.Ok(new { version = version.ToString("s"), qualifier = version.Qualifier });
});
```

`SemVer` can also be created from a string and compared (the qualifier is ignored when comparing):

```csharp
var current = SemVer.FromString("1.4.0-beta");
if (current >= SemVer.FromString("1.2.0"))
{
    // feature is available
}
```

Use `ToString("s")` for `Major.Minor.Patch`; the default format additionally appends the qualifier.

## Modules

The modules below build on each other. The typical setup for a d.velop cloud app is:
`TenantMiddleware` (who is the tenant?) → `IdentityProvider.Middleware` (who is the user?) → `WebApiExtensions` (context and logging) → `Logging.OtelJsonConsole` (log output).
The DTO packages are not documented here, they only contain plain objects.

### Dvelop.Sdk (all-in-one)

Meta-package without code of its own. It references all packages of this SDK, so one `PackageReference` is enough to get the modules below.

### Dvelop.Sdk.BaseInterfaces

Two small interfaces which decouple the other modules from ASP.NET Core:

| Interface         | Members                     | Meaning                                                                        |
|-------------------|-----------------------------|--------------------------------------------------------------------------------|
| `ITenantContext`  | `TenantId`, `SystemBaseUri` | The tenant of the current request                                              |
| `IRequestContext` | `DvRequestId`, `W3CTraceId` | The request id (`x-dv-request-id`) and the W3C trace id of the current request |

Implementations for ASP.NET Core are provided by `Dvelop.Sdk.WebApiExtensions`.

### Dvelop.Sdk.SigningAlgorithms

Static helpers in `HmacSha256Algorithm`. Both return the hash as lowercase hex string.

```csharp
using Dvelop.Sdk.SigningAlgorithms;

var hash = HmacSha256Algorithm.Sha256("some text");
var signature = HmacSha256Algorithm.HmacSha256(secretBytes, "some text");
```

### Dvelop.Sdk.TenantMiddleware

ASP.NET Core middleware which identifies the tenant of a request and verifies that the information really comes from the d.velop cloud.

d.velop cloud sends the headers `x-dv-baseuri` (system base URI), `x-dv-tenant-id` and `x-dv-sig-1`. The signature is the Base64 encoded HMAC-SHA256 of the base URI directly followed by the tenant id, signed with your app's signature secret.

| Request                                              | Result                                              |
|------------------------------------------------------|-----------------------------------------------------|
| Neither base URI nor tenant id header                | `DefaultSystemBaseUri` / `DefaultTenantId` are used |
| Header(s) and valid signature                        | Header values are used                              |
| Header(s) without signature, or with wrong signature | `403 Forbidden`, the next middleware is not called  |
| Header(s) but `SignatureSecretKey` is not configured | `500 Internal Server Error`                         |

```csharp
using Dvelop.Sdk.TenantMiddleware;

app.UseTenantMiddleware(new TenantMiddlewareOptions
{
    SignatureSecretKey = Convert.FromBase64String(builder.Configuration["SignatureSecret"]),
    DefaultSystemBaseUri = "https://my-company.d-velop.cloud",   // used if no header is sent
    DefaultTenantId = "0",
    OnTenantIdentified = (tenantId, systemBaseUri) =>
    {
        // store the tenant for the current request, see Dvelop.Sdk.WebApiExtensions below
    }
});
```

| Option                                    | Description                                                                            |
|-------------------------------------------|----------------------------------------------------------------------------------------|
| `OnTenantIdentified`                      | **Required.** Called with `(tenantId, systemBaseUri)` once the tenant is identified    |
| `SignatureSecretKey`                      | Secret used to verify `x-dv-sig-1`                                                     |
| `AdditionalSignatureSecretKeys`           | Further accepted secrets, e.g. while rotating the secret                               |
| `DefaultSystemBaseUri`, `DefaultTenantId` | Used if the request carries no such header. `DefaultSystemBaseUri` must be a valid URI |
| `IgnoreSignature`                         | Accepts the headers without verifying the signature. **For tests only!**               |
| `LogCallback`                             | Receives log messages (`Debug`, `Info`, `Error`) of the middleware                     |

`TenantMiddlewareHandler` is a `DelegatingHandler` which applies the same rules to outgoing/in-process `HttpClient` requests, e.g. in integration tests.

### Dvelop.Sdk.IdentityProvider.Middleware and Dvelop.Sdk.IdentityProvider.Client

Validates the user session against the d.velop IdentityProvider and sets `HttpContext.User`.

The session id is read from the `AuthSessionId` cookie, or from an `Authorization: Bearer <sessionId>` header. The resulting `ClaimsPrincipal` has the authentication type `d.velop.IdentityProvider` and is cached in memory, so not every request calls the IdentityProvider.

If a request ends with `401 Unauthorized` and has no user, the middleware either redirects the browser to `/identityprovider/login?redirect=<url>` (`GET`/`HEAD` requests which accept `text/html`) or answers with `WWW-Authenticate: Bearer` (everything else, e.g. API calls).

```csharp
using Dvelop.Sdk.IdentityProvider.Middleware;

app.UseTenantMiddleware(...);          // must run first if you use the tenant information
app.UseIdentityProvider(new IdentityProviderOptions
{
    TenantInformationCallback = () => new TenantInformation { TenantId = "...", SystemBaseUri = "https://my-company.d-velop.cloud/" }
});
app.UseAuthorization();
```

To use the standard ASP.NET Core authorization (`[Authorize]`, policies), register the authentication scheme:

```csharp
builder.Services.AddAuthentication("dvelop")
    .AddIdentityProviderAuthentication("dvelop", "d.velop IdentityProvider", options => { });
```

To protect every endpoint unless it carries `[AllowAnonymous]`, additionally set a fallback policy:

```csharp
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
```

| Option (`IdentityProviderOptions`)      | Default            | Description                                                                                                                        |
|-----------------------------------------|--------------------|------------------------------------------------------------------------------------------------------------------------------------|
| `TenantInformationCallback`             | –                  | Returns `TenantInformation` (`TenantId`, `SystemBaseUri`) of the current request. Without it `BaseAddress` and tenant `0` are used |
| `BaseAddress`                           | `http://localhost` | System base URI used if there is no `TenantInformationCallback`                                                                    |
| `HttpClient`                            | `new HttpClient()` | Client used to call the IdentityProvider                                                                                           |
| `AllowExternalValidation`               | `false`            | Allows validation of external users                                                                                                |
| `UseMinimizedOnlyIdValidateDetailLevel` | `false`            | Requests only the user id from the IdentityProvider                                                                                |
| `AllowAppSessions`                      | `true`             | Accepts sessions of other apps                                                                                                     |
| `AllowImpersonatedUsers`                | `true`             | Accepts impersonated users                                                                                                         |
| `AllowedImpersonatedApps`               | empty              | Apps which may impersonate users                                                                                                   |
| `LogCallBack`                           | –                  | Receives log messages (`Debug`, `Info`, `Warning`, `Error`)                                                                        |

`BaseAddress` and `TenantInformation.SystemBaseUri` are concatenated with `identityprovider/...`, so they **must end with `/`** (`https://my-company.d-velop.cloud/`).

`HttpContext.GetAuthSessionId()` returns the session id of a request (cookie first, then bearer header).

`IdentityProviderClient` can also be used without the middleware:

| Method                                         | Purpose                                                                        |
|------------------------------------------------|--------------------------------------------------------------------------------|
| `GetClaimsPrincipalAsync(authSessionId)`       | Validates a session and returns the `ClaimsPrincipal` (cached)                 |
| `GetAuthSessionIdFromApiKey(apiKey)`           | Exchanges an API key for an `AuthSessionInfoDto`, `null` if the key is invalid |
| `QueryAppSessionToken(SessionTokenRequestDto)` | Requests an app session token                                                  |
| `GetLoginUri(redirect)`                        | Relative login URI of the IdentityProvider                                     |

`IdpConst` contains the ids of the built-in IdentityProvider users and groups (built-in admin, tenant admin group, external users, app group).

### Dvelop.Sdk.HttpClientExtensions

`DelegatingHandler`s and extensions for `HttpClient`. They need `ITenantContext` / `IRequestContext` from the DI container.

| Type                                | Purpose                                                                                                                                |
|-------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------|
| `EnsureSystemBaseUriHandler`        | Redirects the request to the `SystemBaseUri` of the current tenant, keeping path and query. Lets you call the cloud with relative URIs |
| `ForwardW3CTraceHandler`            | Adds the `traceparent` header of the current request to outgoing requests                                                              |
| `ForwardDvRequestIdHandler`         | Sends the `x-dv-request-id` of the current request (or the W3C trace id as fallback) with outgoing requests                            |
| `OutgoingHttpRequestLoggingHandler` | Logs start and end of every outgoing request, including status code and duration                                                       |
| `SignWithDv1HmacSha256(secret)`     | Extension for `HttpRequestMessage`. Signs the request with `DV1-HMAC-SHA256`. `secret` is the Base64 encoded app secret                |

```csharp
using Dvelop.Sdk.HttpClientExtensions.DelegatingHandler;
using Dvelop.Sdk.HttpClientExtensions.Extensions.Signing;

builder.Services.AddTransient<EnsureSystemBaseUriHandler>();
builder.Services.AddTransient<ForwardW3CTraceHandler>();
builder.Services.AddHttpClient("dvelop")
    .AddHttpMessageHandler<EnsureSystemBaseUriHandler>()
    .AddHttpMessageHandler<ForwardW3CTraceHandler>();

// signing a single request
var request = new HttpRequestMessage(HttpMethod.Get, "https://my-company.d-velop.cloud/some/api");
await request.SignWithDv1HmacSha256(appSecret);
```

### Dvelop.Sdk.WebApiExtensions

ASP.NET Core building blocks for apps: the context implementations for `Dvelop.Sdk.BaseInterfaces`, middleware for logging and tracing, and request signature validation.

| Type                                                     | Purpose                                                                                                                                                                                                                                                                                                |
|----------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `TenantContext`                                          | `ITenantContext` implementation. Stores `TenantId` and `SystemBaseUri` in `HttpContext.Items`, so it is safe as a singleton. Setters are on the class, the interface is read-only                                                                                                                      |
| `RequestContext`                                         | `IRequestContext` implementation. Reads `x-dv-request-id` and the current W3C trace id                                                                                                                                                                                                                 |
| `W3CTracingMiddleware`                                   | Starts an `Activity` from the `traceparent` header (if none is active) and adds trace and span id to the log scope                                                                                                                                                                                     |
| `TenantLoggingMiddleware`                                | Adds the tenant id to the log scope                                                                                                                                                                                                                                                                    |
| `RequestLoggingMiddleware`                               | Adds the tenant id to the log scope and logs start and end of each incoming request (method, target, user agent, status, duration)                                                                                                                                                                     |
| `HttpRequest.CalculateDv1HmacSha256Signature(appSecret)` | Calculates the `DV1-HMAC-SHA256` signature of an incoming request, to compare with its `Authorization` bearer token. Returns `null` without `x-dv-signature-headers`. It reads the body, so call `request.EnableBuffering()` first. Checking `x-dv-signature-timestamp` (e.g. ±5 minutes) is up to you |
| `HttpRequest.GetRawUrl()`                                | The raw request target as sent by the client                                                                                                                                                                                                                                                           |

Everything is registered manually, there are no `AddXyz` helpers:

```csharp
using Dvelop.Sdk.BaseInterfaces;
using Dvelop.Sdk.WebApiExtensions.Context;
using Dvelop.Sdk.WebApiExtensions.Middleware;

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITenantContext, TenantContext>();
builder.Services.AddSingleton<IRequestContext, RequestContext>();

// TenantContext keeps its values in HttpContext.Items, so every instance created
// from the same accessor sees the same tenant of the current request.
var accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
app.UseTenantMiddleware(new TenantMiddlewareOptions
{
    SignatureSecretKey = ...,
    OnTenantIdentified = (tenantId, systemBaseUri) =>
    {
        var tenant = new TenantContext(accessor);
        tenant.TenantId = tenantId;
        tenant.SystemBaseUri = new Uri(systemBaseUri);
    }
});
app.UseMiddleware<W3CTracingMiddleware>();
app.UseMiddleware<TenantLoggingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
```

Note: this snippet is meant as orientation and has not been compiled as shown.

### Dvelop.Sdk.Logging.Abstractions and Dvelop.Sdk.Logging.OtelJsonConsole

Structured logging in the OTEL-like JSON format of d.velop cloud. See [dvelop-sdk-logging/README.md](dvelop-sdk-logging/README.md) for details and a sample of the log output.

`Logging.Abstractions` has no dependency on a log target. It contains:

| Type                                                                                                               | Purpose                                                                                                                    |
|--------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------|
| `ILogger.LogWithState(level, message, state)`                                                                      | Logs a message together with structured attributes (`CustomLogAttributeState`)                                             |
| `CustomLogAttributeState`                                                                                          | Base class for own attributes. Derive and override `Attributes` (`CustomLogAttributeProperty`, `CustomLogAttributeObject`) |
| `HttpLogAttributeState`, `DatabaseLogAttributeState`, `IncomingHttpRequestLogState`, `OutgoingHttpRequestLogState` | Ready-made states                                                                                                          |
| `TenantLogScope`, `TracingLogScope`                                                                                | Log scopes whose values are written to the tenant and trace fields of the log entry                                        |
| `CustomAttributesLogScope`                                                                                         | Log scope with additional attributes for all entries inside it                                                             |
| `InvisibilityLogScope`                                                                                             | Entries logged inside this scope are marked invisible to customers (`"vis": 0`)                                            |
| `IResourceDescriptor`                                                                                              | Describes service (name, version, instance), host and process. Register it in DI to have it in the `res` field             |

`Logging.OtelJsonConsole` writes these entries as one JSON object per line to the console:

```csharp
using Dvelop.Sdk.Logging.OtelJsonConsole.Extension;

builder.Logging.ClearProviders();
builder.Logging.AddOtelJsonConsole(options => options.IncludeScopes = false);
```

`IncludeScopes = false` only keeps the raw scope dump out of the `attr` field. The tenant id and trace ids (`TenantLogScope`, `TracingLogScope`) and the attributes of a `CustomAttributesLogScope` are written either way.

To fill the `res` field, register an `IResourceDescriptor` in the DI container, e.g. `services.AddSingleton<IResourceDescriptor, MyResourceDescriptor>()`. The formatter picks it up automatically, without one `res` is omitted.

### Dvelop.Sdk.ApplicationInformation

`IVersionProvider` and `SemVer`, see [Reading the application version](#reading-the-application-version-with-iversionprovider) above.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see
the [releases on this repository](https://github.com/d-velop/dvelop-sdk-cs/releases).

## License

Please read [LICENSE](LICENSE) for licensing information.

## Acknowledgments

Thanks to the following projects for inspiration

* [Starting an Open Source Project](https://opensource.guide/starting-a-project/)
* [README template](https://gist.github.com/PurpleBooth/109311bb0361f32d87a2)
* [CONTRIBUTING template](https://github.com/nayafia/contributing-template/blob/master/CONTRIBUTING-template.md)
