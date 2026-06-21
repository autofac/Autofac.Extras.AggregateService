// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Extras.AggregateService.Test.Stubs;

namespace Autofac.Extras.AggregateService.Test.Features;

/// <summary>
/// Verifies the generated path honors generic method constraints.
/// </summary>
public class ConstraintTests
{
    [Fact]
    public void ConstrainedGenericMethodResolvesService()
    {
        using var container = CreateContainer();
        var aggregate = container.Resolve<IConstrainedGenericAggregate>();

        var resolved = aggregate.GetConstrained<MyService>();

        Assert.NotNull(resolved);
        Assert.IsAssignableFrom<IOpenGeneric<MyService>>(resolved);
    }

    private static IContainer CreateContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAggregateService<IConstrainedGenericAggregate>();
        builder.RegisterGeneric(typeof(OpenGeneric<>))
            .As(typeof(IOpenGeneric<>));
        return builder.Build();
    }
}
