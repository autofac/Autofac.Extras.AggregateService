// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test;

public class AggregateServiceGenericsFixture
{
    private readonly IContainer _container;

    public AggregateServiceGenericsFixture()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<IOpenGenericAggregate>();
        builder.RegisterGeneric(typeof(OpenGenericImpl<>))
            .As(typeof(IOpenGeneric<>));
        builder.RegisterGeneric(typeof(PassThroughOpenGenericImpl<>))
            .As(typeof(IPassThroughOpenGeneric<>));

        _container = builder.Build();
    }

    /// <summary>
    /// Attempts to resolve an open generic by a method call.
    /// </summary>
    [Fact]
    public void Method_ResolveOpenGeneric()
    {
        var aggregateService = _container.Resolve<IOpenGenericAggregate>();

        var generic = aggregateService.GetOpenGeneric<object>();
        Assert.NotNull(generic);

        var notGeneric = aggregateService.GetResolvedGeneric();
        Assert.NotNull(notGeneric);
        Assert.NotSame(generic, notGeneric);
    }

    [Fact]
    public void Method_TooManyParameters()
    {
        // Issue #11: Previously with Castle, we ran into parameter limitations.
        // The dynamic proxy fallback maps method parameters onto a
        // System.Func<> delegate, which tops out at 16 type arguments and
        // throws NotSupportedException beyond that. The source generator emits
        // a direct Resolve call with TypedParameter values and has no such
        // limit, so this 18-parameter method now resolves successfully on the
        // generated path.
        var aggregateService = _container.Resolve<IOpenGenericAggregate>();

        var param = aggregateService.GetOpenGeneric<object>();
        Assert.NotNull(param);

        var result = aggregateService.TooManyParameters(param, "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r");
        Assert.NotNull(result);
    }

    [Fact]
    public void Method_WithOpenGenericParameter()
    {
        // Issue #11: A function that takes a generic parameter doesn't use the parameter value.
        var aggregateService = _container.Resolve<IOpenGenericAggregate>();

        var param = aggregateService.GetOpenGeneric<object>();
        Assert.NotNull(param);

        var passThrough = aggregateService.UseOpenGenericParameter(param);
        Assert.Same(param, passThrough.OpenGeneric);
    }
}
