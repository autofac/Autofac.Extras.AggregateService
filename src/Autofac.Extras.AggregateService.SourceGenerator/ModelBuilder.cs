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
    private static readonly SymbolDisplayFormat _fullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    // Fully-qualified name without any type parameter/argument list, so the emitter can build
    // the open (IFoo<,>) and closed (IFoo<T>) forms itself without double-appending.
    private static readonly SymbolDisplayFormat _fullyQualifiedNoTypeParametersFormat =
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
        var typeParameters = definition.TypeParameters.Select(BuildTypeParameter).ToArray();

        var members = TryCollectMembers(definition);
        if (members is null)
        {
            return null;
        }

        var fullyQualified = definition.ToDisplayString(_fullyQualifiedNoTypeParametersFormat);
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
            typeParameters: new EquatableArray<TypeParameterModel>(typeParameters),
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

        // A member can appear more than once across an interface hierarchy (a property shadowed
        // with 'new', or the same-named member declared on two base interfaces). The runtime
        // resolves by type and tolerates this, but the generated class would declare duplicate
        // members (CS0102) / signatures, so de-duplicate by member signature here.
        var seen = new HashSet<string>(StringComparer.Ordinal);

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

                if (seen.Add(BuildMemberSignature(memberModel.Value)))
                {
                    members.Add(memberModel.Value);
                }
            }
        }

        return members;
    }

    // A signature that uniquely identifies an emitted member: name plus, for methods, arity and
    // parameter types (so legitimate overloads are kept but shadowed/duplicate declarations are
    // collapsed). Property names alone are sufficient since C# forbids overloading on them.
    private static string BuildMemberSignature(MemberModel member)
    {
        if (member.Kind == MemberKind.Property)
        {
            return "P:" + member.Name;
        }

        var sb = new System.Text.StringBuilder();
        sb.Append("M:").Append(member.Name).Append('`').Append(member.TypeParameters.Count).Append('(');
        for (var i = 0; i < member.Parameters.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append(member.Parameters[i].Modifier).Append(member.Parameters[i].Type);
        }

        sb.Append(')');
        return sb.ToString();
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

        var propertyType = property.Type.ToDisplayString(_fullyQualifiedFormat);
        return new MemberModel(
            MemberKind.Property,
            property.Name,
            propertyType,
            new EquatableArray<ParameterModel>(Array.Empty<ParameterModel>()),
            new EquatableArray<TypeParameterModel>(Array.Empty<TypeParameterModel>()),
            isGenericMethod: false,
            hasSetter: property.SetMethod is not null);
    }

    private static MemberModel? TryBuildMethod(IMethodSymbol method)
    {
        if (method.MethodKind != MethodKind.Ordinary)
        {
            return null;
        }

        // ref/out parameters cannot be faithfully generated: an 'out' value has nothing to pass
        // to resolution (and would be left unassigned), and 'ref' aliasing cannot be honored by
        // the resolve model. Fall back to the dynamic proxy, which handles ref/out invocation.
        // ('in' and 'params' are supported - the argument value is available to forward.)
        foreach (var parameter in method.Parameters)
        {
            if (parameter.RefKind is RefKind.Ref or RefKind.Out)
            {
                return null;
            }
        }

        var returnType = method.ReturnType.ToDisplayString(_fullyQualifiedFormat);
        var typeParameters = method.TypeParameters.Select(BuildTypeParameter).ToArray();

        // Void methods have no return type to resolve; the runtime throws when they are invoked,
        // so the generated implementation throws too (rather than forcing a whole-interface fallback).
        if (method.ReturnsVoid)
        {
            return new MemberModel(
                MemberKind.ThrowingVoidMethod,
                method.Name,
                "void",
                new EquatableArray<ParameterModel>(BuildParameters(method)),
                new EquatableArray<TypeParameterModel>(typeParameters),
                isGenericMethod: method.IsGenericMethod);
        }

        if (method.Parameters.Length == 0)
        {
            return new MemberModel(
                MemberKind.ParameterlessMethod,
                method.Name,
                returnType,
                new EquatableArray<ParameterModel>(Array.Empty<ParameterModel>()),
                new EquatableArray<TypeParameterModel>(typeParameters),
                isGenericMethod: method.IsGenericMethod);
        }

        return new MemberModel(
            MemberKind.MethodWithParameters,
            method.Name,
            returnType,
            new EquatableArray<ParameterModel>(BuildParameters(method)),
            new EquatableArray<TypeParameterModel>(typeParameters),
            isGenericMethod: method.IsGenericMethod);
    }

    private static ParameterModel[] BuildParameters(IMethodSymbol method)
        => method.Parameters
            .OrderBy(p => p.Ordinal)
            .Select(p => new ParameterModel(
                p.Name,
                p.Type.ToDisplayString(_fullyQualifiedFormat),
                BuildParameterModifier(p)))
            .ToArray();

    private static string BuildParameterModifier(IParameterSymbol parameter)
    {
        if (parameter.IsParams)
        {
            return "params ";
        }

        return parameter.RefKind switch
        {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            _ => string.Empty,
        };
    }

    private static TypeParameterModel BuildTypeParameter(ITypeParameterSymbol typeParameter)
        => new TypeParameterModel(typeParameter.Name, BuildConstraintClause(typeParameter));

    // Builds the comma-separated constraint list (the text after "where T :") in the exact order
    // C# requires: class/struct/notnull/unmanaged first, then base/interface constraints, then
    // new() last. An empty result means the type parameter is unconstrained.
    private static string BuildConstraintClause(ITypeParameterSymbol typeParameter)
    {
        var constraints = new List<string>();

        if (typeParameter.HasReferenceTypeConstraint)
        {
            constraints.Add("class");
        }
        else if (typeParameter.HasValueTypeConstraint)
        {
            constraints.Add(typeParameter.HasUnmanagedTypeConstraint ? "unmanaged" : "struct");
        }
        else if (typeParameter.HasNotNullConstraint)
        {
            constraints.Add("notnull");
        }

        foreach (var constraintType in typeParameter.ConstraintTypes)
        {
            constraints.Add(constraintType.ToDisplayString(_fullyQualifiedFormat));
        }

        // The parameterless-constructor constraint must come last and is not combinable with the
        // struct/unmanaged constraints (which already imply it).
        if (typeParameter.HasConstructorConstraint && !typeParameter.HasValueTypeConstraint)
        {
            constraints.Add("new()");
        }

        return string.Join(", ", constraints);
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
