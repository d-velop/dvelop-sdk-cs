# d.velop cloud SDK for .NET

This is the official SDK to build Apps for [d.velop cloud](https://www.d-velop.de/cloud/) using the C# programming language.

> [!WARNING]
> **Deprecation of the Home-App:** The d.velop Home-App is deprecated and will be replaced by the Dashboard-App.
> The DTOs in `Dvelop.Sdk.Home.Dto` (`FeatureDto`, `FeatureDescriptionDto`, `FeatureBadgeDto`) are marked as `[Obsolete]`
> and will be removed in a future release. The Dashboard-App has a different structure, so there is no one-to-one replacement for these types.

The project has alpha status. **So for now expect things to change.**

## Packages

| Package | Purpose |
|---|---|
| `Dvelop.Sdk` | All-in-one package that references the packages below |
| `Dvelop.Sdk.TenantMiddleware` | Tenant identification and validation of the `x-dv-sig-1` signature |
| `Dvelop.Sdk.IdentityProvider.*` | Auth session validation, user/group DTOs, IdentityProvider middleware and client |
| `Dvelop.Sdk.SigningAlgorithms` | HMAC-SHA256 and DV1-HMAC-SHA256 signing primitives |
| `Dvelop.Sdk.WebApiExtensions` | Request/tenant/W3C-tracing logging middleware, request signing |
| `Dvelop.Sdk.Logging.*` | OTEL JSON console logging |
| `Dvelop.Sdk.HttpClientExtensions` | `HttpClient` helpers |
| DTO packages | Base, CloudCenter, Config, Dashboard and (deprecated) Home DTOs |
| `Dvelop.Sdk.ApplicationInformation` | `IVersionProvider` and `SemVer` (see [below](#reading-the-application-version-with-iversionprovider)) |

## Usage

Include the d.velop sdk packages as dependencies in your .NET 8 project file (`myproject.csproj`) and restore them with `dotnet restore` via commandline or from within your IDE.

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

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Build local

You can build a version of this library with following command:

```bash
dotnet pack -o dist --version-suffix alpha
```

You will need to have an installed and configured dotnet SDK.

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
