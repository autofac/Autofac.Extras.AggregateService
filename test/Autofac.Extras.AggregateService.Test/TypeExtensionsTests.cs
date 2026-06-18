// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test;

public class TypeExtensionsTests
{
    [Fact]
    public void GetUniqueInterfaces_ClassWithInterfaces()
    {
        var interfaces = typeof(Implementer).GetUniqueInterfaces();

        Assert.Contains(typeof(IDerivedInterface), interfaces);
        Assert.Contains(typeof(IBaseInterface), interfaces);
        Assert.DoesNotContain(typeof(Implementer), interfaces);
    }

    [Fact]
    public void GetUniqueInterfaces_ClassWithoutInterfaces()
    {
        var interfaces = typeof(object).GetUniqueInterfaces();

        Assert.Empty(interfaces);
    }

    [Fact]
    public void GetUniqueInterfaces_InterfaceType()
    {
        var interfaces = typeof(IDerivedInterface).GetUniqueInterfaces();

        Assert.Contains(typeof(IDerivedInterface), interfaces);
        Assert.Contains(typeof(IBaseInterface), interfaces);
    }

    private interface IBaseInterface
    {
    }

    private interface IDerivedInterface : IBaseInterface
    {
    }

    private sealed class Implementer : IDerivedInterface
    {
    }
}
