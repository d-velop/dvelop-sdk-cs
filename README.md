# d.velop cloud SDK for .NET core

This is the official SDK to build Apps for [d.velop cloud](https://www.d-velop.de/cloud/) using 
the C# programming language.

The project has alpha status. **So for now expect things to change.** 



## Usage

Include the the d.velop cloud sdk as dependencies in your .NET Core 2.1 project file (`myproject.csproj`) and restore them with `dotnet restore` via commandline or from within your IDE.

```xml
<ItemGroup>
    <PackageReference Include="Dvelop.Sdk.TenantMiddleware" Version="0.0.1.43" />
    <PackageReference Include="Dvelop.Sdk.IdentityProvider.Middleware" Version="0.0.1.43" />
</ItemGroup>
```

or the all-in-one dependency:

```xml
<ItemGroup>
    <PackageReference Include="Dvelop.Sdk" Version="0.0.1.43" />
</ItemGroup>
```

More info on usage of sdk classes and the sources itself will come soon.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct,
and the process for submitting pull requests to us.

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

[![Build Status](https://travis-ci.com/d-velop/dvelop-sdk-cs.svg?branch=master)](https://travis-ci.com/d-velop/dvelop-sdk-cs)