// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Extras.AggregateService.Test.Stubs;
using NSubstitute;

namespace Autofac.Extras.AggregateService.Test.Features;

/// <summary>
/// Verifies the generated path supports <c>in</c> and <c>params</c> method
/// parameter modifiers.
/// </summary>
public class ParameterModifierTests
{
    [Fact]
    public void InParameterMethodResolvesService()
    {
        using var container = CreateContainer();
        var aggregate = container.Resolve<IParameterModifierAggregate>();

        var value = 42;
        var service = aggregate.GetWithIn(in value);

        Assert.NotNull(service);
        Assert.IsAssignableFrom<IMyService>(service);
    }

    [Fact]
    public void ParamsParameterMethodResolvesService()
    {
        using var container = CreateContainer();
        var aggregate = container.Resolve<IParameterModifierAggregate>();

        var service = aggregate.GetWithParams(1, 2, 3);

        Assert.NotNull(service);
        Assert.IsAssignableFrom<IMyService>(service);
    }

    private static IContainer CreateContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<IParameterModifierAggregate>();
        builder.RegisterInstance(Substitute.For<IMyService>());
        return builder.Build();
    }
}
