// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// Describes a generic type parameter (on an interface or method) along with the
/// constraint clause needed to faithfully re-declare it on the generated backing
/// class or method.
/// </summary>
internal readonly struct TypeParameterModel : IEquatable<TypeParameterModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeParameterModel"/> struct.
    /// </summary>
    /// <param name="name">
    /// The type parameter name.
    /// </param>
    /// <param name="constraints">
    /// The comma-separated constraint list (the text following <c>where T :</c>),
    /// or an empty string when the type parameter is unconstrained.
    /// </param>
    public TypeParameterModel(string name, string constraints)
    {
        Name = name;
        Constraints = constraints;
    }

    /// <summary>
    /// Gets the type parameter name.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Gets the comma-separated constraint list (the text following
    /// <c>where T :</c>), or an empty string when unconstrained.
    /// </summary>
    public string Constraints
    {
        get;
    }

    /// <inheritdoc/>
    public bool Equals(TypeParameterModel other) => Name == other.Name && Constraints == other.Constraints;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TypeParameterModel other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => (Name.GetHashCode() * 31) + Constraints.GetHashCode();
}
