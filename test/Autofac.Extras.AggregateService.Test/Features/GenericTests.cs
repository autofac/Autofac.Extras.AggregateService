// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Extras.AggregateService.Test.Stubs;

namespace Autofac.Extras.AggregateService.Test.Features;

public class GenericTests
{
    [Fact]
    public void DeeplyNestedOpenGenericIsNotSupported()
    {
        var builder = new ContainerBuilder();

        // while
        // builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<IOpenGeneric<>>));
        // is not syntactical legal, the following however is.
        var myTrickyType = typeof(IOpenGenericAggregateWithTypeParameter<>).MakeGenericType(typeof(IOpenGeneric<>));

        var action = new Action(() => builder.RegisterAggregateService(myTrickyType));
        Assert.Throws<ArgumentException>(action);
    }

    /// <summary>
    /// Attempts to resolve an open generic by a method call.
    /// </summary>
    [Fact]
    public void MethodResolvesOpenGeneric()
    {
        using var container = CreateAggregateContainer();
        var aggregateService = container.Resolve<IOpenGenericAggregate>();

        var generic = aggregateService.GetOpenGeneric<object>();
        Assert.NotNull(generic);

        var notGeneric = aggregateService.GetResolvedGeneric();
        Assert.NotNull(notGeneric);
        Assert.NotSame(generic, notGeneric);
    }

    [Fact]
    public void MethodWithManyParameters()
    {
        // Issue #11: Previously with Castle, we ran into parameter limitations.
        // The dynamic proxy fallback maps method parameters onto a
        // System.Func<> delegate, which tops out at 16 type arguments and
        // throws NotSupportedException beyond that. The source generator emits
        // a direct Resolve call with TypedParameter values and has no such
        // limit, so this 18-parameter method now resolves successfully on the
        // generated path.
        using var container = CreateAggregateContainer();
        var aggregateService = container.Resolve<IOpenGenericAggregate>();

        var param = aggregateService.GetOpenGeneric<object>();
        Assert.NotNull(param);

        var result = aggregateService.TooManyParameters(param, "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r");
        Assert.NotNull(result);
    }

    [Fact]
    public void MethodWithOpenGenericParameter()
    {
        // Issue #11: A function that takes a generic parameter doesn't use the parameter value.
        using var container = CreateAggregateContainer();
        var aggregateService = container.Resolve<IOpenGenericAggregate>();

        var param = aggregateService.GetOpenGeneric<object>();
        Assert.NotNull(param);

        var passThrough = aggregateService.UseOpenGenericParameter(param);
        Assert.Same(param, passThrough.OpenGeneric);
    }

    [Fact]
    public void ResolvePropertyAsMyService()
    {
        using var container = CreateOpenGenericContainer();
        var aggregateService = container.Resolve<IOpenGenericAggregateWithTypeParameter<IMyService>>();

        var typeOfSomeProperty = aggregateService.SomeProperty.GetType();
        Assert.Same(typeof(MyService), typeOfSomeProperty);

        var generic = aggregateService.OpenGeneric;
        Assert.NotNull(generic);
        var typeOfOpenGeneric = generic.GetType().GetGenericArguments().Single();
        Assert.Same(typeof(IMyService), typeOfOpenGeneric);
    }

    [Fact]
    public void ResolvePropertyAsMyServiceClosed()
    {
        using var container = CreateClosedRegistrationContainer();
        var aggregateService = container.Resolve<IOpenGenericAggregateWithTypeParameter<IMyService>>();

        var typeOfSomeProperty = aggregateService.SomeProperty.GetType();
        Assert.Same(typeof(MyService), typeOfSomeProperty);

        var generic = aggregateService.OpenGeneric;
        Assert.NotNull(generic);
        var typeOfOpenGeneric = generic.GetType().GetGenericArguments().Single();
        Assert.Same(typeof(IMyService), typeOfOpenGeneric);
    }

    [Fact]
    public void ResolvePropertyAsString()
    {
        using var container = CreateOpenGenericContainer();
        var aggregateService = container.Resolve<IOpenGenericAggregateWithTypeParameter<string>>();

        var typeOfSomeProperty = aggregateService.SomeProperty.GetType();
        Assert.Same(typeof(string), typeOfSomeProperty);
        Assert.Same("Hello World!", aggregateService.SomeProperty);

        var generic = aggregateService.OpenGeneric;
        Assert.NotNull(generic);
        var typeOfOpenGeneric = generic.GetType().GetGenericArguments().Single();
        Assert.Same(typeof(string), typeOfOpenGeneric);
    }

    [Fact]
    public void ResolvePropertyAsStringClosed()
    {
        using var container = CreateClosedRegistrationContainer();
        var aggregateService = container.Resolve<IOpenGenericAggregateWithTypeParameter<string>>();

        var typeOfSomeProperty = aggregateService.SomeProperty.GetType();
        Assert.Same(typeof(string), typeOfSomeProperty);
        Assert.Same("Hello World!", aggregateService.SomeProperty);

        var generic = aggregateService.OpenGeneric;
        Assert.NotNull(generic);
        var typeOfOpenGeneric = generic.GetType().GetGenericArguments().Single();
        Assert.Same(typeof(string), typeOfOpenGeneric);
    }

    private static IContainer CreateAggregateContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<IOpenGenericAggregate>();
        builder.RegisterGeneric(typeof(OpenGeneric<>))
            .As(typeof(IOpenGeneric<>));
        builder.RegisterGeneric(typeof(PassThroughOpenGeneric<>))
            .As(typeof(IPassThroughOpenGeneric<>));

        return builder.Build();
    }

    private static IContainer CreateClosedRegistrationContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<string>));
        builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<IMyService>));
        builder.RegisterGeneric(typeof(OpenGeneric<>))
            .As(typeof(IOpenGeneric<>));

        builder.RegisterType<MyService>().As<IMyService>();
        builder.RegisterInstance("Hello World!");

        return builder.Build();
    }

    private static IContainer CreateOpenGenericContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<>));
        builder.RegisterGeneric(typeof(OpenGeneric<>))
            .As(typeof(IOpenGeneric<>));

        builder.RegisterInstance("Hello World!");
        builder.RegisterType<MyService>().As<IMyService>();

        return builder.Build();
    }
}
