// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.SourceGenerator.Test;

/// <summary>
/// Direct unit tests for <see cref="Emitter"/> branches that the full-compilation
/// harness cannot reach - notably the <c>ModuleInitializerAttribute</c> polyfill,
/// which is only emitted when the consuming compilation lacks the attribute (a
/// netstandard2.0 / net472 target), whereas the test host always has it.
/// </summary>
public class EmitterTests
{
    [Fact]
    public void EmitRegistrations_EmitsModuleInitializerPolyfillWhenRequested()
    {
        var model = CreateClosedModel();

        var source = Emitter.EmitRegistrations(new[] { model }, needsModuleInitializerPolyfill: true);

        Assert.Contains("namespace System.Runtime.CompilerServices", source, StringComparison.Ordinal);
        Assert.Contains("class ModuleInitializerAttribute", source, StringComparison.Ordinal);
        Assert.Contains("GeneratedAggregateServiceModuleInitializer", source, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitRegistrations_OmitsModuleInitializerPolyfillWhenNotRequested()
    {
        var model = CreateClosedModel();

        var source = Emitter.EmitRegistrations(new[] { model }, needsModuleInitializerPolyfill: false);

        Assert.DoesNotContain("class ModuleInitializerAttribute", source, StringComparison.Ordinal);
        Assert.Contains("GeneratedAggregateServiceModuleInitializer", source, StringComparison.Ordinal);
    }

    private static AggregateServiceModel CreateClosedModel()
        => new AggregateServiceModel(
            interfaceFullyQualifiedName: "global::TestConsumer.IMyAggregate",
            interfaceNamespace: "TestConsumer",
            interfaceMinimalName: "TestConsumer_IMyAggregate",
            backingClassName: "__TestConsumer_IMyAggregate_Aggregate",
            isOpenGeneric: false,
            typeParameters: new EquatableArray<TypeParameterModel>(System.Array.Empty<TypeParameterModel>()),
            members: new EquatableArray<MemberModel>(System.Array.Empty<MemberModel>()));
}
