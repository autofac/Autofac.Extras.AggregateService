// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.AotSmokeTest;

/// <summary>
/// A leaf dependency exposed as an eagerly-resolved property on the aggregate.
/// </summary>
public interface IFirstService
{
}

/// <summary>
/// A leaf dependency exposed via a resolving method on the aggregate.
/// </summary>
public interface ISecondService
{
}

/// <summary>
/// An aggregate service whose backing implementation is source-generated.
/// </summary>
public interface IMyAggregate
{
    /// <summary>
    /// Gets the first service, resolved eagerly when the aggregate is created.
    /// </summary>
    IFirstService First
    {
        get;
    }

    /// <summary>
    /// Resolves the second service on each call.
    /// </summary>
    /// <returns>
    /// The resolved second service.
    /// </returns>
    ISecondService GetSecond();
}

/// <summary>
/// Concrete implementation of <see cref="IFirstService"/>.
/// </summary>
public sealed class FirstService : IFirstService
{
}

/// <summary>
/// Concrete implementation of <see cref="ISecondService"/>.
/// </summary>
public sealed class SecondService : ISecondService
{
}
