// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// A fully-described aggregate service interface ready for code emission. This
/// model is decoupled from Roslyn symbols so the incremental pipeline can cache
/// it by value.
/// </summary>
internal readonly struct AggregateServiceModel : IEquatable<AggregateServiceModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateServiceModel"/>
    /// struct.
    /// </summary>
    /// <param name="interfaceFullyQualifiedName">
    /// The fully-qualified interface name without any type parameter list.
    /// </param>
    /// <param name="interfaceNamespace">
    /// The namespace the generated backing class is placed in.
    /// </param>
    /// <param name="interfaceMinimalName">
    /// The interface name without namespace, used to build a unique backing
    /// class name.
    /// </param>
    /// <param name="backingClassName">
    /// The simple name of the generated backing class.
    /// </param>
    /// <param name="isOpenGeneric">
    /// Whether the interface is an open generic definition.
    /// </param>
    /// <param name="typeParameters">
    /// The interface's generic type parameters, with their constraints (empty
    /// for non-generic interfaces).
    /// </param>
    /// <param name="members">
    /// The members to implement.
    /// </param>
    public AggregateServiceModel(
        string interfaceFullyQualifiedName,
        string interfaceNamespace,
        string interfaceMinimalName,
        string backingClassName,
        bool isOpenGeneric,
        EquatableArray<TypeParameterModel> typeParameters,
        EquatableArray<MemberModel> members)
    {
        InterfaceFullyQualifiedName = interfaceFullyQualifiedName;
        InterfaceNamespace = interfaceNamespace;
        InterfaceMinimalName = interfaceMinimalName;
        BackingClassName = backingClassName;
        IsOpenGeneric = isOpenGeneric;
        TypeParameters = typeParameters;
        Members = members;
    }

    /// <summary>
    /// Gets the fully-qualified interface name (with <c>global::</c> prefix)
    /// without any type parameter list. The emitter appends the open
    /// (<c>&lt;,&gt;</c>) or closed (<c>&lt;T&gt;</c>) form as needed.
    /// </summary>
    public string InterfaceFullyQualifiedName
    {
        get;
    }

    /// <summary>
    /// Gets the namespace the generated backing class is placed in (the
    /// interface's namespace).
    /// </summary>
    public string InterfaceNamespace
    {
        get;
    }

    /// <summary>
    /// Gets the interface name without namespace, used to build a unique
    /// backing class name.
    /// </summary>
    public string InterfaceMinimalName
    {
        get;
    }

    /// <summary>
    /// Gets the simple name of the generated backing class.
    /// </summary>
    public string BackingClassName
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the interface is an open generic
    /// definition.
    /// </summary>
    public bool IsOpenGeneric
    {
        get;
    }

    /// <summary>
    /// Gets the interface's generic type parameters, with their constraints
    /// (empty for non-generic interfaces).
    /// </summary>
    public EquatableArray<TypeParameterModel> TypeParameters
    {
        get;
    }

    /// <summary>
    /// Gets the members to implement.
    /// </summary>
    public EquatableArray<MemberModel> Members
    {
        get;
    }

    /// <inheritdoc/>
    public bool Equals(AggregateServiceModel other)
        => InterfaceFullyQualifiedName == other.InterfaceFullyQualifiedName
            && InterfaceNamespace == other.InterfaceNamespace
            && InterfaceMinimalName == other.InterfaceMinimalName
            && BackingClassName == other.BackingClassName
            && IsOpenGeneric == other.IsOpenGeneric
            && TypeParameters == other.TypeParameters
            && Members == other.Members;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is AggregateServiceModel other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;
        hash = (hash * 31) + InterfaceFullyQualifiedName.GetHashCode();
        hash = (hash * 31) + InterfaceNamespace.GetHashCode();
        hash = (hash * 31) + InterfaceMinimalName.GetHashCode();
        hash = (hash * 31) + BackingClassName.GetHashCode();
        hash = (hash * 31) + IsOpenGeneric.GetHashCode();
        hash = (hash * 31) + TypeParameters.GetHashCode();
        hash = (hash * 31) + Members.GetHashCode();
        return hash;
    }
}
