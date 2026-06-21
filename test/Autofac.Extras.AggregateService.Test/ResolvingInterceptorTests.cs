// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Extras.AggregateService.Test.Stubs;
using Castle.DynamicProxy;
using NSubstitute;

namespace Autofac.Extras.AggregateService.Test;

public class ResolvingInterceptorTests
{
    [Fact]
    public void Intercept_GenericMethod()
    {
        using var container = CreateGenericContainer();
        var interceptor = new ResolvingInterceptor(typeof(IOpenGenericAggregate), container);
        var method = GetSingleMethod<IOpenGenericAggregate>(nameof(IOpenGenericAggregate.GetOpenGeneric)).MakeGenericMethod(typeof(string));
        var invocation = CreateInvocation(method);

        interceptor.Intercept(invocation);

        Assert.IsType<OpenGeneric<string>>(invocation.ReturnValue);
    }

    [Fact]
    public void Intercept_MethodWithGenericParameter()
    {
        using var container = CreateGenericContainer();
        var interceptor = new ResolvingInterceptor(typeof(IOpenGenericAggregate), container);
        var method = GetSingleMethod<IOpenGenericAggregate>(nameof(IOpenGenericAggregate.UseOpenGenericParameter)).MakeGenericMethod(typeof(string));
        var openGeneric = new OpenGeneric<string>();
        var invocation = CreateInvocation(method, openGeneric);

        interceptor.Intercept(invocation);

        var passThrough = Assert.IsType<PassThroughOpenGeneric<string>>(invocation.ReturnValue);
        Assert.Same(openGeneric, passThrough.OpenGeneric);
    }

    [Fact]
    public void Intercept_MethodWithParameters()
    {
        var someDependency = Substitute.For<ISomeDependency>();
        using var container = CreateAggregateContainer(someDependency);
        var interceptor = new ResolvingInterceptor(typeof(IAggregateService), container);
        var method = GetMethod<IAggregateService>(nameof(IAggregateService.GetMyService), typeof(DateTime), typeof(int));
        var someDate = DateTime.Now;
        var invocation = CreateInvocation(method, someDate, 20);

        interceptor.Intercept(invocation);

        var myService = Assert.IsAssignableFrom<IMyService>(invocation.ReturnValue);
        Assert.Equal(someDate, myService.SomeDateValue);
        Assert.Equal(20, myService.SomeIntValue);
        Assert.Same(someDependency, myService.SomeDependency);
    }

    [Fact]
    public void Intercept_NullInvocation()
    {
        using var container = CreateAggregateContainer(Substitute.For<ISomeDependency>());
        var interceptor = new ResolvingInterceptor(typeof(IAggregateService), container);

        Assert.Throws<ArgumentNullException>(() => interceptor.Intercept(null!));
    }

    [Fact]
    public void Intercept_PropertyGetter()
    {
        using var container = CreateAggregateContainer(Substitute.For<ISomeDependency>());
        var interceptor = new ResolvingInterceptor(typeof(IAggregateService), container);
        var method = typeof(IAggregateService).GetProperty(nameof(IAggregateService.MyService))!.GetGetMethod()!;
        var invocation = CreateInvocation(method);

        interceptor.Intercept(invocation);

        Assert.IsAssignableFrom<IMyService>(invocation.ReturnValue);
    }

    [Fact]
    public void Intercept_TooManyParameters()
    {
        using var container = CreateGenericContainer();
        var interceptor = new ResolvingInterceptor(typeof(IOpenGenericAggregate), container);
        var method = GetSingleMethod<IOpenGenericAggregate>(nameof(IOpenGenericAggregate.TooManyParameters)).MakeGenericMethod(typeof(string));
        var openGeneric = new OpenGeneric<string>();
        var invocation = CreateInvocation(method, openGeneric, "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r");

        Assert.Throws<NotSupportedException>(() => interceptor.Intercept(invocation));
    }

    [Fact]
    public void Intercept_VoidMethod()
    {
        using var container = CreateAggregateContainer(Substitute.For<ISomeDependency>());
        var interceptor = new ResolvingInterceptor(typeof(IAggregateService), container);
        var method = GetMethod<IAggregateService>(nameof(IAggregateService.MethodWithoutReturnValue));
        var invocation = CreateInvocation(method);

        Assert.Throws<InvalidOperationException>(() => interceptor.Intercept(invocation));
    }

    [Fact]
    public void Intercept_WithoutParameters()
    {
        using var container = CreateAggregateContainer(Substitute.For<ISomeDependency>());
        var interceptor = new ResolvingInterceptor(typeof(IAggregateService), container);
        var method = GetMethod<IAggregateService>(nameof(IAggregateService.GetMyService));
        var invocation = CreateInvocation(method);

        interceptor.Intercept(invocation);

        Assert.IsAssignableFrom<IMyService>(invocation.ReturnValue);
    }

    private static IContainer CreateAggregateContainer(ISomeDependency someDependency)
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(someDependency);
        builder.RegisterType<MyService>()
            .As<IMyService>()
            .InstancePerDependency();
        return builder.Build();
    }

    private static IContainer CreateGenericContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(OpenGeneric<>))
            .As(typeof(IOpenGeneric<>));
        builder.RegisterInstance<Func<IOpenGeneric<string>, IPassThroughOpenGeneric<string>>>(openGeneric => new PassThroughOpenGeneric<string>(openGeneric));
        return builder.Build();
    }

    private static IInvocation CreateInvocation(MethodInfo method, params object?[] arguments)
    {
        var invocation = Substitute.For<IInvocation>();
        invocation.Method.Returns(method);
        invocation.Arguments.Returns(arguments);
        return invocation;
    }

    private static MethodInfo GetMethod<T>(string name, params Type[] parameterTypes)
    {
        return typeof(T).GetMethod(name, parameterTypes) ?? throw new InvalidOperationException($"Unable to locate {typeof(T).Name}.{name}.");
    }

    private static MethodInfo GetSingleMethod<T>(string name)
    {
        return typeof(T).GetMethods().Single(method => method.Name == name);
    }
}
