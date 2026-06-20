// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.AotSmokeTest;

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

    /// <summary>
    /// Resolves the second service while forwarding a parameter (exercises the
    /// generated TypedParameter path under AOT).
    /// </summary>
    /// <param name="seed">
    /// A parameter forwarded to resolution.
    /// </param>
    /// <returns>
    /// The resolved second service.
    /// </returns>
    ISecondService GetSecond(int seed);
}
