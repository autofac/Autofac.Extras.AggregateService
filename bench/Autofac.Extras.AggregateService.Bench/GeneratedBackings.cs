// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.CompilerServices;

namespace Autofac.Extras.AggregateService.Bench;

// Hand-written stand-ins for the code the source generator will emit. These exist to
// validate the linking seam (registry + module initializer) and to benchmark the generated
// path against the Castle DynamicProxy path before the generator itself is built.
//
// Semantics intentionally mirror ResolvingInterceptor:
//   * Properties are resolved eagerly in the constructor (same instance every access).
//   * Parameterless methods resolve the return type on each call.
//   * Methods with parameters resolve the return type passing TypedParameter values.
internal sealed class GeneratedAggregateImpl : IGeneratedAggregate
{
    private readonly IComponentContext _context;

    public GeneratedAggregateImpl(IComponentContext context)
    {
        _context = context;
        ServiceA = _context.Resolve<IServiceA>();
        ServiceB = _context.Resolve<IServiceB>();
    }

    public IServiceA ServiceA
    {
        get;
    }

    public IServiceB ServiceB
    {
        get;
    }

    public IServiceA GetServiceA()
    {
        return _context.Resolve<IServiceA>();
    }

    public IServiceC GetServiceC(int number)
    {
        return _context.Resolve<IServiceC>(new TypedParameter(typeof(int), number));
    }
}

internal sealed class GeneratedOpenAggregateImpl<T> : IGeneratedOpenAggregate<T>
{
    public GeneratedOpenAggregateImpl(IComponentContext context)
    {
        Item = (T)context.Resolve(typeof(T));
    }

    public T Item
    {
        get;
    }
}

internal static class GeneratedRegistrations
{
    [ModuleInitializer]
    internal static void Register()
    {
        GeneratedAggregateServiceRegistry.Register(
            typeof(IGeneratedAggregate),
            context => new GeneratedAggregateImpl(context));

        GeneratedAggregateServiceRegistry.RegisterOpenGeneric(
            typeof(IGeneratedOpenAggregate<>),
            typeof(GeneratedOpenAggregateImpl<>));
    }
}
