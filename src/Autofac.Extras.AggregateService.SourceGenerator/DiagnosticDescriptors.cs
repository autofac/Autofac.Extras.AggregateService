// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// Diagnostics reported by the aggregate service source generator.
/// </summary>
internal static class DiagnosticDescriptors
{
    /// <summary>
    /// Reported (informational) when an aggregate service interface contains a
    /// member shape the generator does not emit (for example, an event or an
    /// indexer). Generation is skipped for that interface and resolution falls
    /// back to the Castle DynamicProxy implementation at runtime, so behavior
    /// is preserved.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsupportedMemberFallsBackToProxy = new DiagnosticDescriptor(
        id: "AGSVC001",
        title: "Aggregate service uses the dynamic proxy fallback",
        messageFormat: "Aggregate service interface '{0}' contains a member ('{1}') that the source generator does not support; it will use the Castle DynamicProxy fallback at runtime",
        category: "Autofac.Extras.AggregateService.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
