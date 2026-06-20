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
    /// Reported (informational) when an aggregate service registration targets an
    /// interface the generator cannot emit a backing class for - for example one
    /// containing an event, an indexer, or a <c>ref</c>/<c>out</c> method
    /// parameter. Generation is skipped for that interface and resolution falls
    /// back to the Castle DynamicProxy implementation at runtime (which is not
    /// trimming/NativeAOT safe), so behavior is preserved but the AOT benefit is
    /// lost for that interface.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsupportedInterfaceFallsBackToProxy = new DiagnosticDescriptor(
        id: "AGSVC001",
        title: "Aggregate service uses the dynamic proxy fallback",
        messageFormat: "Aggregate service interface '{0}' contains a member shape the source generator does not support; it will use the Castle DynamicProxy fallback at runtime, which is not trimming/NativeAOT safe",
        category: "Autofac.Extras.AggregateService.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
