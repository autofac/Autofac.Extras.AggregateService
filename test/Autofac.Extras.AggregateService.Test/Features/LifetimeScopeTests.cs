// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Extras.AggregateService.Test.Stubs;
using NSubstitute;

namespace Autofac.Extras.AggregateService.Test.Features;

/// <summary>
/// Verifies that generated aggregate services resolve their dependencies from the
/// lifetime scope they are resolved in, not just the root container - important
/// because the generated backing class resolves properties eagerly from the
/// injected component context.
/// </summary>
public class LifetimeScopeTests
{
    [Fact]
    public void ResolvesFromChildScope()
    {
        using var container = CreateContainer();
        using var scope = container.BeginLifetimeScope();

        var aggregate = scope.Resolve<IAggregateService>();

        Assert.NotNull(aggregate.MyService);
    }

    [Fact]
    public void MethodResolvesScopedDependencyFromOwningScope()
    {
        using var container = CreateContainer();

        // A dependency registered InstancePerLifetimeScope should resolve to the instance
        // belonging to the scope the aggregate was resolved from.
        using var scope = container.BeginLifetimeScope();
        var aggregate = scope.Resolve<IAggregateService>();
        var scopedService = scope.Resolve<IMyService>();

        var fromAggregate = aggregate.GetMyService();

        Assert.Same(scopedService, fromAggregate);
    }

    private static IContainer CreateContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<IAggregateService>();
        builder.RegisterInstance(Substitute.For<ISomeDependency>());
        builder.Register(_ => Substitute.For<IMyService>())
            .As<IMyService>()
            .InstancePerLifetimeScope();
        return builder.Build();
    }
}
