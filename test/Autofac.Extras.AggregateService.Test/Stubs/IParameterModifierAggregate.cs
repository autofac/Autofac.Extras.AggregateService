// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test.Stubs;

/// <summary>
/// An aggregate service exercising <c>in</c> and <c>params</c> parameter
/// modifiers, both of which the source generator supports.
/// </summary>
public interface IParameterModifierAggregate
{
    /// <summary>
    /// Resolves a service while taking an <c>in</c> parameter.
    /// </summary>
    /// <param name="value">
    /// The readonly-reference parameter forwarded to resolution.
    /// </param>
    /// <returns>
    /// The resolved service.
    /// </returns>
    IMyService GetWithIn(in int value);

    /// <summary>
    /// Resolves a service while taking a <c>params</c> parameter.
    /// </summary>
    /// <param name="values">
    /// The params array forwarded to resolution.
    /// </param>
    /// <returns>
    /// The resolved service.
    /// </returns>
    IMyService GetWithParams(params int[] values);
}
