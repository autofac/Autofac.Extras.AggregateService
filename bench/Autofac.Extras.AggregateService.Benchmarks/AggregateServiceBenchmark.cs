// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Benchmarks;

/// <summary>
/// Compares the source-generated aggregate service path against the Castle DynamicProxy
/// fallback for the common operations: resolving the aggregate, reading eagerly-resolved
/// properties, and invoking resolving methods (with and without parameters).
/// </summary>
[MemoryDiagnoser]
public class AggregateServiceBenchmark
{
    private IContainer _container = null!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<ServiceA>().As<IServiceA>().InstancePerDependency();
        builder.RegisterType<ServiceB>().As<IServiceB>().InstancePerDependency();
        builder.RegisterType<ServiceC>().As<IServiceC>().InstancePerDependency();

        // The generated path: a statically-visible registration the source generator can see,
        // so it emits a backing class and the registry resolves directly to it.
        builder.RegisterAggregateService<IGeneratedAggregate>();
        builder.RegisterAggregateService(typeof(IGeneratedOpenAggregate<>));

        // The Castle fallback path: the same registration API, but the interface type is obtained
        // at runtime so the generator can't trace it statically. These resolve through the dynamic
        // proxy instead of a generated backing.
        builder.RegisterAggregateService(HideFromGenerator(typeof(IProxiedAggregate)));
        builder.RegisterAggregateService(HideFromGenerator(typeof(IProxiedOpenAggregate<>)));

        _container = builder.Build();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _container.Dispose();
    }

    [Benchmark(Baseline = true)]
    public IServiceA Castle_Resolve_And_Property()
    {
        var aggregate = _container.Resolve<IProxiedAggregate>();
        return aggregate.ServiceA;
    }

    [Benchmark]
    public IServiceA Generated_Resolve_And_Property()
    {
        var aggregate = _container.Resolve<IGeneratedAggregate>();
        return aggregate.ServiceA;
    }

    [Benchmark]
    public IServiceA Castle_MethodNoParams()
    {
        var aggregate = _container.Resolve<IProxiedAggregate>();
        return aggregate.GetServiceA();
    }

    [Benchmark]
    public IServiceA Generated_MethodNoParams()
    {
        var aggregate = _container.Resolve<IGeneratedAggregate>();
        return aggregate.GetServiceA();
    }

    [Benchmark]
    public IServiceC Castle_MethodWithParams()
    {
        var aggregate = _container.Resolve<IProxiedAggregate>();
        return aggregate.GetServiceC(5);
    }

    [Benchmark]
    public IServiceC Generated_MethodWithParams()
    {
        var aggregate = _container.Resolve<IGeneratedAggregate>();
        return aggregate.GetServiceC(5);
    }

    [Benchmark]
    public IServiceA Castle_OpenGeneric()
    {
        var aggregate = _container.Resolve<IProxiedOpenAggregate<IServiceA>>();
        return aggregate.Item;
    }

    [Benchmark]
    public IServiceA Generated_OpenGeneric()
    {
        var aggregate = _container.Resolve<IGeneratedOpenAggregate<IServiceA>>();
        return aggregate.Item;
    }

    // Returns the type as-is, but routes it through a method call so the source generator's
    // static analysis can't see the concrete typeof(...) and therefore does not emit a backing
    // class - forcing the Castle DynamicProxy fallback at runtime.
    private static Type HideFromGenerator(Type interfaceType) => interfaceType;
}
