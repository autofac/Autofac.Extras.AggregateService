// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// The kind of member encountered on an aggregate service interface, mirroring
/// the dispatch decisions made at runtime by the <c>ResolvingInterceptor</c>.
/// </summary>
internal enum MemberKind
{
    /// <summary>
    /// A property whose getter returns a value resolved eagerly in the
    /// constructor. A setter, if present, throws (matching the runtime, which
    /// rejects property setters).
    /// </summary>
    Property,

    /// <summary>
    /// A parameterless method that resolves its return type on each call.
    /// </summary>
    ParameterlessMethod,

    /// <summary>
    /// A method with parameters that are forwarded to resolution as typed
    /// parameters.
    /// </summary>
    MethodWithParameters,

    /// <summary>
    /// A method with a <c>void</c> return type, which throws on invocation
    /// (matching the runtime, which has no return type to resolve).
    /// </summary>
    ThrowingVoidMethod,
}
