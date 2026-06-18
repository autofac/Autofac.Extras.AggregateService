// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Bench;

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

// The aggregate service shape used by both the generated and the Castle benchmarks.
// Two identically-shaped interfaces are used so the registry can hold a generated
// implementation for one while the other falls back to the dynamic proxy.
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

public interface ICastleAggregate
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

// Open generic aggregate shapes, again paired so one can be generated and one proxied.
public interface IGeneratedOpenAggregate<T>
{
    T Item
    {
        get;
    }
}

public interface ICastleOpenAggregate<T>
{
    T Item
    {
        get;
    }
}
