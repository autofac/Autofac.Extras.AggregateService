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
    public ParameterModel(string name, string type)
    {
        Name = name;
        Type = type;
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

    /// <inheritdoc/>
    public bool Equals(ParameterModel other) => Name == other.Name && Type == other.Type;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ParameterModel other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => (Name.GetHashCode() * 31) + Type.GetHashCode();
}
