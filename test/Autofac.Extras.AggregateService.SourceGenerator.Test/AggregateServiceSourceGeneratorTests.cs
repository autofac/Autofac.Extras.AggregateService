// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.SourceGenerator.Test;

public class AggregateServiceSourceGeneratorTests
{
    [Fact]
    public Task RegisterAggregateService_GenericForm()
    {
        var source = Wrap(@"
public interface IMyService { }

public interface IMyAggregate
{
    IMyService MyService { get; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IMyAggregate>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task RegisterAggregateService_TypeofForm()
    {
        var source = Wrap(@"
public interface IMyService { }

public interface IMyAggregate
{
    IMyService MyService { get; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService(typeof(IMyAggregate));
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task CreateInstance_GenericForm()
    {
        var source = Wrap(@"
public interface IMyService { }

public interface IMyAggregate
{
    IMyService MyService { get; }
}

public static class Registration
{
    public static object Configure(IComponentContext context)
    {
        return AggregateServiceGenerator.CreateInstance<IMyAggregate>(context);
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task CreateInstance_TypeofForm()
    {
        var source = Wrap(@"
public interface IMyService { }

public interface IMyAggregate
{
    IMyService MyService { get; }
}

public static class Registration
{
    public static object Configure(IComponentContext context)
    {
        return AggregateServiceGenerator.CreateInstance(typeof(IMyAggregate), context);
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task Properties_ResolvedEagerly_SetterThrows()
    {
        var source = Wrap(@"
public interface IMyService { }

public interface IMyAggregate
{
    IMyService ReadOnly { get; }
    IMyService WithSetter { get; set; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IMyAggregate>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task Methods_AllShapes()
    {
        var source = Wrap(@"
public interface IMyService { }

public interface IMyAggregate
{
    IMyService GetService();
    IMyService GetService(int value);
    IMyService GetService(System.DateTime date, string name);
    void DoNothing();
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IMyAggregate>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task GenericMethods()
    {
        var source = Wrap(@"
public interface IThing<T> { }

public interface IMyAggregate
{
    IThing<T> Get<T>();
    IThing<T> Use<T>(IThing<T> existing);
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IMyAggregate>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task OpenGenericInterface()
    {
        var source = Wrap(@"
public interface IThing<T> { }

public interface IMyAggregate<T>
{
    T Item { get; }
    IThing<T> Thing { get; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService(typeof(IMyAggregate<>));
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task InheritedInterfaceMembers()
    {
        var source = Wrap(@"
public interface IFirst { }
public interface ISecond { }

public interface IBaseAggregate
{
    IFirst First { get; }
}

public interface IMyAggregate : IBaseAggregate
{
    ISecond Second { get; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IMyAggregate>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task MultipleAggregates_DeduplicatedAcrossCallSites()
    {
        var source = Wrap(@"
public interface IMyService { }

public interface IMyAggregate
{
    IMyService MyService { get; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder, IComponentContext context)
    {
        // The same aggregate referenced from multiple call sites should emit only once.
        builder.RegisterAggregateService<IMyAggregate>();
        builder.RegisterAggregateService(typeof(IMyAggregate));
        AggregateServiceGenerator.CreateInstance<IMyAggregate>(context);
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task NoAggregateServices_EmitsNothing()
    {
        var source = Wrap(@"
public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterType<object>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task GenericMethodConstraints()
    {
        var source = Wrap(@"
public interface IThing<T> { }
public interface IDep { }

public interface IMyAggregate
{
    IThing<T> Get<T>() where T : class, new();
    IThing<TKey> Multi<TKey, TValue>() where TKey : struct where TValue : IDep;
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IMyAggregate>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task OpenGenericInterfaceWithConstraint()
    {
        var source = Wrap(@"
public interface IMyAggregate<T>
    where T : class
{
    T Item { get; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService(typeof(IMyAggregate<>));
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task MultiArityOpenGenericInterface()
    {
        var source = Wrap(@"
public interface IMyAggregate<TKey, TValue>
{
    TKey Key { get; }
    TValue Value { get; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService(typeof(IMyAggregate<,>));
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task InAndParamsParameters()
    {
        var source = Wrap(@"
public interface IMyService { }

public interface IMyAggregate
{
    IMyService WithIn(in int value);
    IMyService WithParams(params int[] values);
    IMyService WithDefault(int value = 5);
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IMyAggregate>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task RefAndOutParameters_FallBackToProxy()
    {
        // ref/out parameters cannot be faithfully generated, so the generator emits nothing and
        // reports AGSVC001; the runtime falls back to the dynamic proxy.
        var source = Wrap(@"
public interface IMyService { }

public interface IMyAggregate
{
    IMyService WithOut(out int value);
    IMyService WithRef(ref int value);
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IMyAggregate>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task ShadowedAndDuplicateInheritedMembers()
    {
        var source = Wrap(@"
public interface IDep { }

public interface IBase
{
    IDep Shared { get; }
}

public interface IMyAggregate : IBase
{
    new IDep Shared { get; }
    IDep Other { get; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IMyAggregate>();
    }
}");

        return Verify(GeneratorTestHarness.Run(source));
    }

    [Fact]
    public Task NamespacelessInterface()
    {
        // An aggregate interface declared in the global namespace exercises the emitter's
        // no-namespace branch.
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

public interface IGlobalService { }

public interface IGlobalAggregate
{
    IGlobalService Service { get; }
}

public static class Registration
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<IGlobalAggregate>();
    }
}";

        return Verify(GeneratorTestHarness.Run(source));
    }

    /// <summary>
    /// Wraps a snippet in the using directives and namespace shared by all test sources.
    /// </summary>
    /// <param name="body">
    /// The body source to wrap.
    /// </param>
    /// <returns>
    /// The full compilation unit source.
    /// </returns>
    private static string Wrap(string body)
    {
        return @"using System;
using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
" + body + @"
}";
    }
}
