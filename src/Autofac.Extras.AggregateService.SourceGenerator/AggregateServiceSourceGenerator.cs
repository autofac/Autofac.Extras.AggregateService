// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// Incremental source generator that emits concrete backing implementations for
/// aggregate service interfaces.
/// </summary>
/// <remarks>
/// <para>
/// The generator discovers aggregate service interfaces by scanning the
/// consuming compilation for the registration / creation call sites that the
/// library exposes:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///     <c>ContainerBuilderExtensions.RegisterAggregateService&lt;T&gt;()</c>
///     </description>
///   </item>
///   <item>
///     <description>
///     <c>ContainerBuilderExtensions.RegisterAggregateService(typeof(T))</c>
///     </description>
///   </item>
///   <item>
///     <description>
///     <c>AggregateServiceGenerator.CreateInstance&lt;T&gt;(context)</c>
///     </description>
///   </item>
///   <item>
///     <description>
///     <c>AggregateServiceGenerator.CreateInstance(typeof(T), context)</c>
///     </description>
///   </item>
/// </list>
/// <para>
/// For each statically-resolvable interface it emits a backing class plus a
/// module initializer that registers a factory with the
/// <c>GeneratedAggregateServiceRegistry</c>. At runtime
/// <c>AggregateServiceGenerator.CreateInstance</c> uses the registered factory
/// when present and otherwise falls back to Castle DynamicProxy, so call sites
/// that pass a runtime-computed <see cref="System.Type"/> (which the generator
/// cannot see) keep working.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class AggregateServiceSourceGenerator : IIncrementalGenerator
{
    private const string RegisterAggregateServiceMethod = "RegisterAggregateService";
    private const string CreateInstanceMethod = "CreateInstance";
    private const string ContainerBuilderExtensionsType = "Autofac.Extras.AggregateService.ContainerBuilderExtensions";
    private const string AggregateServiceGeneratorType = "Autofac.Extras.AggregateService.AggregateServiceGenerator";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var results = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateInvocation(node),
                transform: static (ctx, ct) => GetDiscoveryResult(ctx, ct))
            .Where(static result => result is not null)
            .Select(static (result, _) => result!.Value);

        // Collect so identical interfaces referenced from multiple call sites emit only once.
        var collected = results.Collect();

        // The generated module initializer needs System.Runtime.CompilerServices.ModuleInitializerAttribute.
        // It exists in-box on net5.0+ but not on netstandard2.0 / net472 consumers, so detect its
        // absence and emit an internal polyfill in that case (the C# 9+ compiler recognizes it by
        // full name regardless of target framework).
        var needsModuleInitializerPolyfill = context.CompilationProvider
            .Select(static (compilation, _) =>
                compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ModuleInitializerAttribute") is null);

        var combined = collected.Combine(needsModuleInitializerPolyfill);

        context.RegisterSourceOutput(combined, static (spc, pair) => Emit(spc, pair.Left, pair.Right));
    }

    private static bool IsCandidateInvocation(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        var name = invocation.Expression switch
        {
            MemberAccessExpressionSyntax member => member.Name,
            MemberBindingExpressionSyntax binding => binding.Name,
            _ => null,
        };

        var identifier = name switch
        {
            GenericNameSyntax generic => generic.Identifier.ValueText,
            IdentifierNameSyntax id => id.Identifier.ValueText,
            _ => null,
        };

        return identifier is RegisterAggregateServiceMethod or CreateInstanceMethod;
    }

    private static DiscoveryResult? GetDiscoveryResult(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        var invocation = (InvocationExpressionSyntax)ctx.Node;
        var semanticModel = ctx.SemanticModel;

        if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol method)
        {
            return null;
        }

        var containingType = method.ContainingType?.ToDisplayString();
        if (containingType is not (ContainerBuilderExtensionsType or AggregateServiceGeneratorType))
        {
            return null;
        }

        // Extract the interface type symbol from either the generic type argument or a
        // typeof(...) argument, whichever form the call site used.
        var interfaceType = ResolveInterfaceType(method, invocation, semanticModel, cancellationToken);
        if (interfaceType is null)
        {
            // An aggregate registration whose type is not statically visible (e.g. a
            // runtime-computed Type, or an open type parameter from a generic pass-through
            // helper). The generator cannot see it; the runtime falls back silently. This is
            // not reported - there is no specific interface to name.
            return null;
        }

        var model = ModelBuilder.TryBuild(interfaceType);
        if (model is not null)
        {
            return DiscoveryResult.Supported(model.Value);
        }

        // The interface is identifiable but has a shape the generator cannot emit (event,
        // indexer, ref/out parameter, etc.). Record it so AGSVC001 can surface the silent
        // fallback to the consumer.
        return DiscoveryResult.Unsupported(
            interfaceType.ToDisplayString(),
            LocationInfo.CreateFrom(invocation));
    }

    private static INamedTypeSymbol? ResolveInterfaceType(
        IMethodSymbol method,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        // Generic form: RegisterAggregateService<T>() / CreateInstance<T>(context).
        if (method.IsGenericMethod && method.TypeArguments.Length == 1)
        {
            return method.TypeArguments[0] as INamedTypeSymbol;
        }

        // typeof form: the interface comes from a typeof(...) argument.
        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (argument.Expression is TypeOfExpressionSyntax typeOf)
            {
                var typeInfo = semanticModel.GetTypeInfo(typeOf.Type, cancellationToken);
                return typeInfo.Type as INamedTypeSymbol;
            }
        }

        return null;
    }

    private static void Emit(
        SourceProductionContext context,
        ImmutableArray<DiscoveryResult> results,
        bool needsModuleInitializerPolyfill)
    {
        if (results.IsDefaultOrEmpty)
        {
            return;
        }

        // De-duplicate by the interface's fully-qualified (open) name. The same aggregate
        // service is commonly registered and resolved from several call sites.
        var seenModels = new HashSet<string>(StringComparer.Ordinal);
        var unique = new List<AggregateServiceModel>();

        // Report each unsupported interface once, at its first-seen call site.
        var reportedUnsupported = new HashSet<string>(StringComparer.Ordinal);

        foreach (var result in results)
        {
            if (result.Model is { } model)
            {
                if (seenModels.Add(model.InterfaceFullyQualifiedName))
                {
                    unique.Add(model);
                }

                continue;
            }

            var interfaceName = result.UnsupportedInterfaceName!;
            if (reportedUnsupported.Add(interfaceName))
            {
                var location = result.Location?.ToLocation() ?? Location.None;
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedInterfaceFallsBackToProxy,
                    location,
                    interfaceName));
            }
        }

        foreach (var model in unique)
        {
            var source = Emitter.EmitBackingClass(model);
            context.AddSource($"{model.BackingClassName}.g.cs", source);
        }

        if (unique.Count > 0)
        {
            var registrations = Emitter.EmitRegistrations(unique, needsModuleInitializerPolyfill);
            context.AddSource("GeneratedAggregateServiceRegistrations.g.cs", registrations);
        }
    }
}
