// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Extras.AggregateService.Test.Stubs;
using NSubstitute;

namespace Autofac.Extras.AggregateService.Test;

public class AggregateServiceGeneratorTests
{
    [Fact]
    public void CreateInstance_InterfaceType()
    {
        using var container = CreateContainer();
        var instance = AggregateServiceGenerator.CreateInstance(typeof(IAggregateService), container);
        Assert.IsAssignableFrom<IAggregateService>(instance);
    }

    [Fact]
    public void CreateInstance_NestedInterfaceType()
    {
        using var container = CreateContainer();

        var instance = AggregateServiceGenerator.CreateInstance(typeof(INestedAggregateService), container);

        var aggregateService = Assert.IsAssignableFrom<INestedAggregateService>(instance);
        Assert.NotNull(aggregateService.MyService);
    }

    [Fact]
    public void CreateInstance_NullComponentContext()
    {
        Assert.Throws<ArgumentNullException>(() => AggregateServiceGenerator.CreateInstance(typeof(IAggregateService), null!));
    }

    [Fact]
    public void CreateInstance_NullInterfaceType()
    {
        using var container = CreateContainer();
        Assert.Throws<ArgumentNullException>(() => AggregateServiceGenerator.CreateInstance(null!, container));
    }

    [Fact]
    public void CreateInstance_T_InterfaceType()
    {
        using var container = CreateContainer();
        var instance = AggregateServiceGenerator.CreateInstance<IAggregateService>(container);
        Assert.IsAssignableFrom<IAggregateService>(instance);
    }

    [Fact]
    public void CreateInstance_T_NonInterfaceType()
    {
        using var container = CreateContainer();
        Assert.Throws<ArgumentException>(() => AggregateServiceGenerator.CreateInstance<string>(container));
    }

    private static IContainer CreateContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Substitute.For<IMyService>());
        return builder.Build();
    }

    public interface INestedAggregateService
    {
        IMyService MyService
        {
            get;
        }
    }
}
