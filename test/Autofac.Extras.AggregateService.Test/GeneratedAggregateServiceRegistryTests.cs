// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NSubstitute;

namespace Autofac.Extras.AggregateService.Test;

public class GeneratedAggregateServiceRegistryTests
{
    [Fact]
    public void Register_NullFactory()
    {
        Assert.Throws<ArgumentNullException>(() => GeneratedAggregateServiceRegistry.Register(typeof(IClosedAggregate), null!));
    }

    [Fact]
    public void Register_NullInterfaceType()
    {
        Assert.Throws<ArgumentNullException>(() => GeneratedAggregateServiceRegistry.Register(null!, _ => new ClosedAggregate()));
    }

    [Fact]
    public void RegisterOpenGeneric_NullBackingType()
    {
        Assert.Throws<ArgumentNullException>(() => GeneratedAggregateServiceRegistry.RegisterOpenGeneric(typeof(IOpenAggregate<>), null!));
    }

    [Fact]
    public void RegisterOpenGeneric_NullInterfaceType()
    {
        Assert.Throws<ArgumentNullException>(() => GeneratedAggregateServiceRegistry.RegisterOpenGeneric(null!, typeof(OpenAggregate<>)));
    }

    [Fact]
    public void TryCreate_ClosedFactoryRegistered()
    {
        var expected = new ClosedAggregate();
        var context = Substitute.For<IComponentContext>();
        GeneratedAggregateServiceRegistry.Register(typeof(IClosedAggregate), receivedContext =>
        {
            Assert.Same(context, receivedContext);
            return expected;
        });

        var created = GeneratedAggregateServiceRegistry.TryCreate(typeof(IClosedAggregate), context, out var instance);

        Assert.True(created);
        Assert.Same(expected, instance);
    }

    [Fact]
    public void TryCreate_NoRegisteredFactory()
    {
        var context = Substitute.For<IComponentContext>();

        var created = GeneratedAggregateServiceRegistry.TryCreate(typeof(IUnregisteredAggregate), context, out var instance);

        Assert.False(created);
        Assert.Null(instance);
    }

    [Fact]
    public void TryCreate_OpenGenericBackingRegistered()
    {
        var context = Substitute.For<IComponentContext>();
        GeneratedAggregateServiceRegistry.RegisterOpenGeneric(typeof(IOpenAggregate<>), typeof(OpenAggregate<>));

        var created = GeneratedAggregateServiceRegistry.TryCreate(typeof(IOpenAggregate<string>), context, out var instance);

        var aggregate = Assert.IsType<OpenAggregate<string>>(instance);
        Assert.True(created);
        Assert.Same(context, aggregate.Context);
    }

    public interface IClosedAggregate
    {
    }

    public interface IOpenAggregate<T>
    {
    }

    public interface IUnregisteredAggregate
    {
    }

    public sealed class ClosedAggregate : IClosedAggregate
    {
    }

    public sealed class OpenAggregate<T> : IOpenAggregate<T>
    {
        public OpenAggregate(IComponentContext context)
        {
            Context = context;
        }

        public IComponentContext Context
        {
            get;
        }
    }
}
