// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Extras.AggregateService.Test.Stubs;
using NSubstitute;

namespace Autofac.Extras.AggregateService.Test.Features;

/// <summary>
/// Verifies that interfaces the source generator cannot emit (here, a method with
/// a <c>ref</c> parameter) still resolve correctly via the Castle DynamicProxy
/// fallback, and that the registry/fallback seam behaves end to end.
/// </summary>
public class FallbackTests
{
    [Fact]
    public void RefParameterMethodResolvesViaProxyFallback()
    {
        using var container = CreateContainer();

        var aggregate = container.Resolve<IRefOutAggregate>();
        var value = 0;
        var service = aggregate.GetWithRef(ref value);

        Assert.NotNull(service);
        Assert.IsAssignableFrom<IMyService>(service);
    }

    [Fact]
    public void RuntimeComputedTypeResolvesViaProxyFallback()
    {
        // A registration whose Type is computed at runtime is invisible to the generator, so it
        // must still resolve through the dynamic proxy fallback.
        var builder = new ContainerBuilder();
        var interfaceType = typeof(IRefOutAggregate);
        builder.RegisterAggregateService(interfaceType);
        builder.RegisterInstance(Substitute.For<IMyService>());
        using var container = builder.Build();

        var resolved = container.Resolve(interfaceType);

        Assert.IsAssignableFrom<IRefOutAggregate>(resolved);
    }

    private static IContainer CreateContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<IRefOutAggregate>();
        builder.RegisterInstance(Substitute.For<IMyService>());
        return builder.Build();
    }
}
