// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// A value-equatable surrogate for a <see cref="Location"/>. Raw
/// <see cref="Location"/> instances are not ideal to carry through the
/// incremental pipeline (they reduce cache hits), so the discovery transform
/// captures this lightweight form and reconstructs a <see cref="Location"/> only
/// when a diagnostic is actually reported.
/// </summary>
internal readonly struct LocationInfo : IEquatable<LocationInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocationInfo"/> struct.
    /// </summary>
    /// <param name="filePath">
    /// The source file path.
    /// </param>
    /// <param name="textSpan">
    /// The span within the source text.
    /// </param>
    /// <param name="lineSpan">
    /// The line/column span within the source text.
    /// </param>
    public LocationInfo(string filePath, TextSpan textSpan, LinePositionSpan lineSpan)
    {
        FilePath = filePath;
        TextSpan = textSpan;
        LineSpan = lineSpan;
    }

    /// <summary>
    /// Gets the source file path.
    /// </summary>
    public string FilePath
    {
        get;
    }

    /// <summary>
    /// Gets the span within the source text.
    /// </summary>
    public TextSpan TextSpan
    {
        get;
    }

    /// <summary>
    /// Gets the line/column span within the source text.
    /// </summary>
    public LinePositionSpan LineSpan
    {
        get;
    }

    /// <summary>
    /// Creates a <see cref="LocationInfo"/> from a syntax node, or
    /// <see langword="null"/> if the node has no usable location.
    /// </summary>
    /// <param name="node">
    /// The syntax node whose location should be captured.
    /// </param>
    /// <returns>
    /// The captured location, or <see langword="null"/>.
    /// </returns>
    public static LocationInfo? CreateFrom(SyntaxNode node)
    {
        var location = node.GetLocation();
        if (location.SourceTree is null)
        {
            return null;
        }

        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    /// <summary>
    /// Reconstructs a <see cref="Location"/> for diagnostic reporting.
    /// </summary>
    /// <returns>
    /// The reconstructed <see cref="Location"/>.
    /// </returns>
    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

    /// <inheritdoc/>
    public bool Equals(LocationInfo other)
        => FilePath == other.FilePath && TextSpan == other.TextSpan && LineSpan.Equals(other.LineSpan);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is LocationInfo other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;
        hash = (hash * 31) + FilePath.GetHashCode();
        hash = (hash * 31) + TextSpan.GetHashCode();
        hash = (hash * 31) + LineSpan.GetHashCode();
        return hash;
    }
}
