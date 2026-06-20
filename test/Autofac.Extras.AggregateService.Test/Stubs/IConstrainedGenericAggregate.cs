// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test.Stubs;

/// <summary>
/// An aggregate service with a constrained generic method, exercising the
/// generator's emission of <c>where</c> constraint clauses.
/// </summary>
public interface IConstrainedGenericAggregate
{
    /// <summary>
    /// Resolves an open generic constrained to reference types with a public
    /// parameterless constructor.
    /// </summary>
    /// <typeparam name="T">
    /// The constrained type argument.
    /// </typeparam>
    /// <returns>
    /// The resolved open generic.
    /// </returns>
    IOpenGeneric<T> GetConstrained<T>()
        where T : class, new();
}
