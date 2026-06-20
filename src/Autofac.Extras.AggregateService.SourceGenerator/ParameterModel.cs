// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// Describes a single parameter of an aggregate service method.
/// </summary>
internal readonly struct ParameterModel : IEquatable<ParameterModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterModel"/> struct.
    /// </summary>
    /// <param name="name">
    /// The parameter name.
    /// </param>
    /// <param name="type">
    /// The fully-qualified parameter type as a C# type expression.
    /// </param>
    /// <param name="modifier">
    /// The leading parameter modifier including a trailing space (for example
    /// <c>"ref "</c>, <c>"in "</c>, <c>"out "</c>, <c>"params "</c>), or an empty
    /// string for a by-value parameter.
    /// </param>
    public ParameterModel(string name, string type, string modifier)
    {
        Name = name;
        Type = type;
        Modifier = modifier;
    }

    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Gets the fully-qualified parameter type (as a C# type expression).
    /// </summary>
    public string Type
    {
        get;
    }

    /// <summary>
    /// Gets the leading parameter modifier including a trailing space (for
    /// example <c>"ref "</c>, <c>"in "</c>, <c>"out "</c>, <c>"params "</c>), or
    /// an empty string for a by-value parameter.
    /// </summary>
    public string Modifier
    {
        get;
    }

    /// <inheritdoc/>
    public bool Equals(ParameterModel other) => Name == other.Name && Type == other.Type && Modifier == other.Modifier;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ParameterModel other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;
        hash = (hash * 31) + Name.GetHashCode();
        hash = (hash * 31) + Type.GetHashCode();
        hash = (hash * 31) + Modifier.GetHashCode();
        return hash;
    }
}
