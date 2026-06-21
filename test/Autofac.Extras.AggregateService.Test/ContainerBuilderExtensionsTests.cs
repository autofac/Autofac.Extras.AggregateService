// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Extras.AggregateService.Test.Stubs;
using NSubstitute;

namespace Autofac.Extras.AggregateService.Test;

public class ContainerBuilderExtensionsTests
{
    [Fact]
    public void RegisterAggregateService_DifferentLifetimeScopes()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService(typeof(IAggregateService));
        builder.RegisterType<MyService>()
            .As<IMyService>()
            .InstancePerLifetimeScope();
        var container = builder.Build();

        var rootScope = container.Resolve<IAggregateService>();
        var subScope = container.BeginLifetimeScope().Resolve<IAggregateService>();

        Assert.NotSame(subScope.MyService, rootScope.MyService);
    }

    [Fact]
    public void RegisterAggregateService_InterfaceType()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService(typeof(IAggregateService));
        var container = builder.Build();

        Assert.True(container.IsRegistered<IAggregateService>());
    }

    [Fact]
    public void RegisterAggregateService_NonInterfaceType()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentException>(() => builder.RegisterAggregateService(typeof(MyService)));
    }

    [Fact]
    public void RegisterAggregateService_NullInterfaceType()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.RegisterAggregateService(null!));
    }

    [Fact]
    public void RegisterAggregateService_PerDependencyScope()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<IAggregateService>();
        builder.RegisterInstance(Substitute.For<IMyService>());
        var container = builder.Build();

        var firstInstance = container.Resolve<IAggregateService>();
        var secondInstance = container.Resolve<IAggregateService>();

        Assert.NotSame(secondInstance, firstInstance);
    }

    [Fact]
    public void RegisterAggregateService_T_InterfaceType()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<IAggregateService>();
        var container = builder.Build();

        Assert.True(container.IsRegistered<IAggregateService>());
    }

    [Fact]
    public void RegisterAggregateService_T_NonInterfaceType()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentException>(() => builder.RegisterAggregateService<MyService>());
    }
}
