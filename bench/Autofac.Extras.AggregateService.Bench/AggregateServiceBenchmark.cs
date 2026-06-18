// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Bench;

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

        // IGeneratedAggregate has a generated backing registered via module initializer;
        // ICastleAggregate has none, so it exercises the DynamicProxy fallback.
        builder.RegisterAggregateService<IGeneratedAggregate>();
        builder.RegisterAggregateService<ICastleAggregate>();

        builder.RegisterAggregateService(typeof(IGeneratedOpenAggregate<>));
        builder.RegisterAggregateService(typeof(ICastleOpenAggregate<>));

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
        var aggregate = _container.Resolve<ICastleAggregate>();
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
        var aggregate = _container.Resolve<ICastleAggregate>();
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
        var aggregate = _container.Resolve<ICastleAggregate>();
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
        var aggregate = _container.Resolve<ICastleOpenAggregate<IServiceA>>();
        return aggregate.Item;
    }

    [Benchmark]
    public IServiceA Generated_OpenGeneric()
    {
        var aggregate = _container.Resolve<IGeneratedOpenAggregate<IServiceA>>();
        return aggregate.Item;
    }
}
