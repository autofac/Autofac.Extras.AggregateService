// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Benchmarks;

// Leaf dependencies the aggregate services pull together.
public interface IServiceA
{
    int Value
    {
        get;
    }
}

public interface IServiceB
{
    string Name
    {
        get;
    }
}

public interface IServiceC
{
    int Number
    {
        get;
    }
}

public sealed class ServiceA : IServiceA
{
    public int Value => 42;
}

public sealed class ServiceB : IServiceB
{
    public string Name => "service-b";
}

public sealed class ServiceC : IServiceC
{
    private readonly int _number;

    public ServiceC(int number)
    {
        _number = number;
    }

    public int Number => _number;
}

// The aggregate service shape exercised by the benchmarks. Two identically-shaped interfaces
// let one be driven down the source-generated path (registered with a statically-visible
// RegisterAggregateService<T>(), so the generator emits a backing class) and the other down the
// Castle DynamicProxy fallback (registered via a runtime-obtained Type the generator can't see).
public interface IGeneratedAggregate
{
    IServiceA ServiceA
    {
        get;
    }

    IServiceB ServiceB
    {
        get;
    }

    IServiceA GetServiceA();

    IServiceC GetServiceC(int number);
}

public interface IProxiedAggregate
{
    IServiceA ServiceA
    {
        get;
    }

    IServiceB ServiceB
    {
        get;
    }

    IServiceA GetServiceA();

    IServiceC GetServiceC(int number);
}

// Open generic aggregate shapes, again paired so one is generated and one proxied.
public interface IGeneratedOpenAggregate<T>
{
    T Item
    {
        get;
    }
}

public interface IProxiedOpenAggregate<T>
{
    T Item
    {
        get;
    }
}
