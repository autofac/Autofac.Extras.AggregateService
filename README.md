# Autofac.Extras.AggregateService

Dynamic aggregate service implementation generation for [Autofac](https://autofac.org) via Castle DynamicProxy.

[![Build status](https://github.com/autofac/Autofac.Extras.AggregateService/actions/workflows/main.yml/badge.svg)](https://github.com/autofac/Autofac.Extras.AggregateService/actions/workflows/main.yml) [![codecov](https://codecov.io/gh/Autofac/Autofac.Extras.AggregateService/branch/develop/graph/badge.svg)](https://codecov.io/gh/Autofac/Autofac.Extras.AggregateService) [![NuGet](https://img.shields.io/nuget/v/Autofac.Extras.AggregateService.svg)](https://nuget.org/packages/Autofac.Extras.AggregateService)

Please file issues and pull requests for this package in this repository rather than in the Autofac core repo.

- [Documentation](https://autofac.readthedocs.io/en/latest/advanced/aggregate-services.html)
- [NuGet](https://www.nuget.org/packages/Autofac.Extras.AggregateService/)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)
- [Open in Visual Studio Code](https://open.vscode.dev/autofac/Autofac.Extras.AggregateService)

## Quick Start

Once you've added a reference to the `Autofac.Pooling` package, you can start using the new `PooledInstancePerLifetimeScope` and `PooledInstancePerMatchingLifetimeScope` methods:

```csharp
var builder = new ContainerBuilder();
```

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).
