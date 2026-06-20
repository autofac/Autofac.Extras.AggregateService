// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test.Stubs;

/// <summary>
/// An aggregate service with a <c>ref</c> parameter. The source generator cannot
/// emit a faithful implementation for <c>ref</c>/<c>out</c> parameters, so this
/// interface exercises the Castle DynamicProxy fallback at runtime.
/// </summary>
public interface IRefOutAggregate
{
    /// <summary>
    /// Resolves a service while taking a <c>ref</c> parameter (forces the
    /// dynamic proxy fallback).
    /// </summary>
    /// <param name="value">
    /// A reference parameter that is read but not meaningfully used.
    /// </param>
    /// <returns>
    /// The resolved service.
    /// </returns>
    IMyService GetWithRef(ref int value);
}
