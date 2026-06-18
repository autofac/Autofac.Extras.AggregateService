// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// Builds an <see cref="AggregateServiceModel"/> from a Roslyn interface
/// symbol, mirroring the member categorization performed at runtime by the
/// <c>ResolvingInterceptor</c>.
/// </summary>
internal static class ModelBuilder
{
    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    // Fully-qualified name without any type parameter/argument list, so the emitter can build
    // the open (IFoo<,>) and closed (IFoo<T>) forms itself without double-appending.
    private static readonly SymbolDisplayFormat FullyQualifiedNoTypeParametersFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);

    /// <summary>
    /// Attempts to build a model for the given interface symbol. Returns
    /// <see langword="null"/> when the interface contains a shape the generator
    /// does not support, in which case the runtime falls back to the dynamic
    /// proxy.
    /// </summary>
    /// <param name="interfaceType">
    /// The aggregate service interface symbol.
    /// </param>
    /// <returns>
    /// The model, or <see langword="null"/> if generation should be skipped.
    /// </returns>
    public static AggregateServiceModel? TryBuild(INamedTypeSymbol interfaceType)
    {
        if (interfaceType.TypeKind != TypeKind.Interface)
        {
            return null;
        }

        // The generator works against the open generic definition so a single backing class
        // serves all closed constructions. Closed constructions seen at a call site are mapped
        // back to their definition here.
        var definition = interfaceType.IsGenericType ? interfaceType.ConstructedFrom : interfaceType;

        // A type nested inside a generic, or otherwise containing free type parameters that are
        // not its own definition parameters, is not supported (mirrors the runtime guard against
        // deeply-nested open generics).
        if (definition.ContainingType is not null)
        {
            return null;
        }

        var isOpenGeneric = definition.IsGenericType;
        var typeParameterNames = definition.TypeParameters.Select(tp => tp.Name).ToArray();

        var members = TryCollectMembers(definition);
        if (members is null)
        {
            return null;
        }

        var fullyQualified = definition.ToDisplayString(FullyQualifiedNoTypeParametersFormat);
        var ns = definition.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : definition.ContainingNamespace.ToDisplayString();
        var minimalName = BuildMinimalName(definition);
        var backingClassName = BuildBackingClassName(definition);

        return new AggregateServiceModel(
            interfaceFullyQualifiedName: fullyQualified,
            interfaceNamespace: ns,
            interfaceMinimalName: minimalName,
            backingClassName: backingClassName,
            isOpenGeneric: isOpenGeneric,
            typeParameters: new EquatableArray<string>(typeParameterNames),
            members: new EquatableArray<MemberModel>(members.ToArray()));
    }

    /// <summary>
    /// Collects the member models for the interface and all interfaces it
    /// inherits, returning <see langword="null"/> if any member shape is
    /// unsupported (forcing the runtime fallback).
    /// </summary>
    private static List<MemberModel>? TryCollectMembers(INamedTypeSymbol definition)
    {
        var members = new List<MemberModel>();

        // GetUniqueInterfaces equivalent: the interface itself plus all inherited interfaces.
        var allInterfaces = new List<INamedTypeSymbol> { definition };
        allInterfaces.AddRange(definition.AllInterfaces);

        foreach (var interfaceSymbol in allInterfaces)
        {
            foreach (var member in interfaceSymbol.GetMembers())
            {
                var memberModel = TryBuildMemberSymbol(member);
                if (memberModel is null)
                {
                    if (member is IMethodSymbol method && IsAccessor(method))
                    {
                        // Property get/set accessors are handled via the property symbol; skip.
                        continue;
                    }

                    return null;
                }

                members.Add(memberModel.Value);
            }
        }

        return members;
    }

    private static MemberModel? TryBuildMemberSymbol(ISymbol member)
        => member switch
        {
            IPropertySymbol property => TryBuildProperty(property),
            IMethodSymbol method when IsAccessor(method) => null,
            IMethodSymbol method => TryBuildMethod(method),
            _ => null,
        };

    private static bool IsAccessor(IMethodSymbol method)
        => method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet;

    private static MemberModel? TryBuildProperty(IPropertySymbol property)
    {
        // Indexers cannot be resolved as a single service.
        if (property.IsIndexer)
        {
            return null;
        }

        // A property must be readable: the runtime resolves the getter's return type eagerly.
        if (property.GetMethod is null)
        {
            return null;
        }

        var propertyType = property.Type.ToDisplayString(FullyQualifiedFormat);
        return new MemberModel(
            MemberKind.Property,
            property.Name,
            propertyType,
            new EquatableArray<ParameterModel>(Array.Empty<ParameterModel>()),
            new EquatableArray<string>(Array.Empty<string>()),
            isGenericMethod: false,
            hasSetter: property.SetMethod is not null);
    }

    private static MemberModel? TryBuildMethod(IMethodSymbol method)
    {
        if (method.MethodKind != MethodKind.Ordinary)
        {
            return null;
        }

        var returnType = method.ReturnType.ToDisplayString(FullyQualifiedFormat);
        var typeParameters = method.TypeParameters.Select(tp => tp.Name).ToArray();

        // Void methods have no return type to resolve; the runtime throws when they are invoked,
        // so the generated implementation throws too (rather than forcing a whole-interface fallback).
        if (method.ReturnsVoid)
        {
            return new MemberModel(
                MemberKind.ThrowingVoidMethod,
                method.Name,
                "void",
                new EquatableArray<ParameterModel>(
                    method.Parameters
                        .OrderBy(p => p.Ordinal)
                        .Select(p => new ParameterModel(p.Name, p.Type.ToDisplayString(FullyQualifiedFormat)))
                        .ToArray()),
                new EquatableArray<string>(typeParameters),
                isGenericMethod: method.IsGenericMethod);
        }

        if (method.Parameters.Length == 0)
        {
            return new MemberModel(
                MemberKind.ParameterlessMethod,
                method.Name,
                returnType,
                new EquatableArray<ParameterModel>(Array.Empty<ParameterModel>()),
                new EquatableArray<string>(typeParameters),
                isGenericMethod: method.IsGenericMethod);
        }

        var parameters = method.Parameters
            .OrderBy(p => p.Ordinal)
            .Select(p => new ParameterModel(p.Name, p.Type.ToDisplayString(FullyQualifiedFormat)))
            .ToArray();

        return new MemberModel(
            MemberKind.MethodWithParameters,
            method.Name,
            returnType,
            new EquatableArray<ParameterModel>(parameters),
            new EquatableArray<string>(typeParameters),
            isGenericMethod: method.IsGenericMethod);
    }

    private static string BuildMinimalName(INamedTypeSymbol definition)
    {
        // A stable, identifier-safe fragment unique to this interface, including namespace so
        // two same-named interfaces in different namespaces do not collide.
        var ns = definition.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : definition.ContainingNamespace.ToDisplayString().Replace('.', '_') + "_";

        var arity = definition.TypeParameters.Length > 0 ? "_" + definition.TypeParameters.Length : string.Empty;
        return ns + definition.Name + arity;
    }

    private static string BuildBackingClassName(INamedTypeSymbol definition)
        => "__" + BuildMinimalName(definition) + "_Aggregate";
}
