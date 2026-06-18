// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Extras.AggregateService.Test.Stubs;
using NSubstitute;

namespace Autofac.Extras.AggregateService.Test.Features;

public class AggregateServiceTests
{
    [Fact]
    public void PropertyResolvesService()
    {
        using var container = CreateContainer();
        var aggregateService = container.Resolve<IAggregateService>();

        var service = aggregateService.MyService;
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IMyService>(service);
    }

    [Fact]
    public void PropertyGetterReturnsSameInstance()
    {
        using var container = CreateContainer();
        var aggregateService = container.Resolve<IAggregateService>();

        var firstInstance = aggregateService.MyService;
        var secondInstance = aggregateService.MyService;

        Assert.Same(secondInstance, firstInstance);
    }

    [Fact]
    public void PropertySetterIsUnsupported()
    {
        using var container = CreateContainer();
        var aggregateService = container.Resolve<IAggregateService>();

        Assert.Throws<InvalidOperationException>(() => aggregateService.PropertyWithSetter = null!);
    }

    [Fact]
    public void VoidMethodIsUnsupported()
    {
        using var container = CreateContainer();
        var aggregateService = container.Resolve<IAggregateService>();

        Assert.Throws<InvalidOperationException>(() => aggregateService.MethodWithoutReturnValue());
    }

    [Fact]
    public void MethodResolvesService()
    {
        using var container = CreateContainer();
        var aggregateService = container.Resolve<IAggregateService>();

        var service = aggregateService.GetMyService();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IMyService>(service);
    }

    [Fact]
    public void MethodWithParameterPassesParameterToService()
    {
        using var container = CreateContainer();
        var aggregateService = container.Resolve<IAggregateService>();

        var myService = aggregateService.GetMyService(10);

        Assert.Equal(10, myService.SomeIntValue);
    }

    [Fact]
    public void MethodWithParametersPassesParametersToService()
    {
        using var container = CreateContainer();
        var aggregateService = container.Resolve<IAggregateService>();
        var someDate = DateTime.Now;

        var myService = aggregateService.GetMyService(someDate, 20);

        Assert.Equal(someDate, myService.SomeDateValue);
        Assert.Equal(20, myService.SomeIntValue);
    }

    [Fact]
    public void MethodWithNullParameterPassesParameterToService()
    {
        using var container = CreateContainer();
        var aggregateService = container.Resolve<IAggregateService>();

        var myService = aggregateService.GetMyService(null);

        Assert.Null(myService.SomeStringValue);
    }

    [Fact]
    public void MethodWithParameterPassesParameterAndOtherDependenciesToService()
    {
        var someDependency = Substitute.For<ISomeDependency>();
        using var container = CreateContainer(someDependency);
        var aggregateService = container.Resolve<IAggregateService>();

        var myService = aggregateService.GetMyService("text");

        Assert.Equal("text", myService.SomeStringValue);
        Assert.Equal(someDependency, myService.SomeDependency);
    }

    private static IContainer CreateContainer(ISomeDependency? someDependency = null)
    {
        someDependency ??= Substitute.For<ISomeDependency>();
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<IAggregateService>();
        builder.RegisterType<MyService>()
            .As<IMyService>()
            .InstancePerDependency();
        builder.RegisterInstance(someDependency);
        return builder.Build();
    }
}
