// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// Describes one member (property or method) to emit on the generated backing
/// class.
/// </summary>
internal readonly struct MemberModel : IEquatable<MemberModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberModel"/> struct.
    /// </summary>
    /// <param name="kind">
    /// The member kind dispatch category.
    /// </param>
    /// <param name="name">
    /// The member name (property or method name).
    /// </param>
    /// <param name="returnType">
    /// The fully-qualified return / property type as a C# type expression.
    /// </param>
    /// <param name="parameters">
    /// The method parameters (empty for properties and parameterless methods).
    /// </param>
    /// <param name="typeParameters">
    /// The generic type parameters declared by the method, with their
    /// constraints (empty if none).
    /// </param>
    /// <param name="isGenericMethod">
    /// Whether this member is a generic method.
    /// </param>
    /// <param name="hasSetter">
    /// Whether a property member also declares a setter (which is emitted to
    /// throw).
    /// </param>
    public MemberModel(
        MemberKind kind,
        string name,
        string returnType,
        EquatableArray<ParameterModel> parameters,
        EquatableArray<TypeParameterModel> typeParameters,
        bool isGenericMethod,
        bool hasSetter = false)
    {
        Kind = kind;
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        TypeParameters = typeParameters;
        IsGenericMethod = isGenericMethod;
        HasSetter = hasSetter;
    }

    /// <summary>
    /// Gets the member kind dispatch category.
    /// </summary>
    public MemberKind Kind
    {
        get;
    }

    /// <summary>
    /// Gets the member name (property or method name).
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Gets the fully-qualified return / property type (as a C# type
    /// expression).
    /// </summary>
    public string ReturnType
    {
        get;
    }

    /// <summary>
    /// Gets the method parameters (empty for properties and parameterless
    /// methods).
    /// </summary>
    public EquatableArray<ParameterModel> Parameters
    {
        get;
    }

    /// <summary>
    /// Gets the generic type parameters declared by the method, with their
    /// constraints (empty if none).
    /// </summary>
    public EquatableArray<TypeParameterModel> TypeParameters
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether this member is a generic method.
    /// </summary>
    public bool IsGenericMethod
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether a property member also declares a setter
    /// (which is emitted to throw).
    /// </summary>
    public bool HasSetter
    {
        get;
    }

    /// <inheritdoc/>
    public bool Equals(MemberModel other)
        => Kind == other.Kind
            && Name == other.Name
            && ReturnType == other.ReturnType
            && Parameters == other.Parameters
            && TypeParameters == other.TypeParameters
            && IsGenericMethod == other.IsGenericMethod
            && HasSetter == other.HasSetter;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MemberModel other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;
        hash = (hash * 31) + (int)Kind;
        hash = (hash * 31) + Name.GetHashCode();
        hash = (hash * 31) + ReturnType.GetHashCode();
        hash = (hash * 31) + Parameters.GetHashCode();
        hash = (hash * 31) + TypeParameters.GetHashCode();
        hash = (hash * 31) + IsGenericMethod.GetHashCode();
        hash = (hash * 31) + HasSetter.GetHashCode();
        return hash;
    }
}
