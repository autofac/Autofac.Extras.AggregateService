// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Extras.AggregateService.Test.Stubs;
using NSubstitute;

namespace Autofac.Extras.AggregateService.Test.Features;

public class InheritanceTests
{
    [Fact]
    public void ResolvePropertyOnSuperType()
    {
        var someDependency = Substitute.For<ISomeDependency>();
        using var container = CreateContainer(someDependency, Substitute.For<ISomeOtherDependency>());
        var aggregateService = container.Resolve<ISubService>();

        Assert.Equal(someDependency, aggregateService.SomeDependency);
    }

    [Fact]
    public void ResolvePropertyOnSubType()
    {
        var someOtherDependency = Substitute.For<ISomeOtherDependency>();
        using var container = CreateContainer(Substitute.For<ISomeDependency>(), someOtherDependency);
        var aggregateService = container.Resolve<ISubService>();

        Assert.Equal(someOtherDependency, aggregateService.SomeOtherDependency);
    }

    private static IContainer CreateContainer(ISomeDependency someDependency, ISomeOtherDependency someOtherDependency)
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<ISubService>();
        builder.RegisterInstance(someDependency);
        builder.RegisterInstance(someOtherDependency);
        return builder.Build();
    }
}
