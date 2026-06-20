// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// The outcome of inspecting one aggregate service registration / creation call
/// site: either a model to emit, or a record that the interface is unsupported
/// and will fall back to the dynamic proxy (which is reported as AGSVC001).
/// </summary>
internal readonly struct DiscoveryResult : IEquatable<DiscoveryResult>
{
    private DiscoveryResult(AggregateServiceModel? model, string? unsupportedInterfaceName, LocationInfo? location)
    {
        Model = model;
        UnsupportedInterfaceName = unsupportedInterfaceName;
        Location = location;
    }

    /// <summary>
    /// Gets the model to emit, or <see langword="null"/> when the interface is
    /// unsupported.
    /// </summary>
    public AggregateServiceModel? Model
    {
        get;
    }

    /// <summary>
    /// Gets the display name of the unsupported interface, or
    /// <see langword="null"/> when a model was produced.
    /// </summary>
    public string? UnsupportedInterfaceName
    {
        get;
    }

    /// <summary>
    /// Gets the call-site location used to report the AGSVC001 diagnostic, or
    /// <see langword="null"/> when a model was produced or no location is
    /// available.
    /// </summary>
    public LocationInfo? Location
    {
        get;
    }

    /// <summary>
    /// Creates a result carrying a model to emit.
    /// </summary>
    /// <param name="model">
    /// The model to emit.
    /// </param>
    /// <returns>
    /// A supported-interface discovery result.
    /// </returns>
    public static DiscoveryResult Supported(AggregateServiceModel model)
        => new DiscoveryResult(model, unsupportedInterfaceName: null, location: null);

    /// <summary>
    /// Creates a result recording that an interface is unsupported and will fall
    /// back to the dynamic proxy.
    /// </summary>
    /// <param name="interfaceName">
    /// The display name of the unsupported interface.
    /// </param>
    /// <param name="location">
    /// The call-site location for the diagnostic.
    /// </param>
    /// <returns>
    /// An unsupported-interface discovery result.
    /// </returns>
    public static DiscoveryResult Unsupported(string interfaceName, LocationInfo? location)
        => new DiscoveryResult(model: null, interfaceName, location);

    /// <inheritdoc/>
    public bool Equals(DiscoveryResult other)
        => Nullable.Equals(Model, other.Model)
            && UnsupportedInterfaceName == other.UnsupportedInterfaceName
            && Nullable.Equals(Location, other.Location);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DiscoveryResult other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;
        hash = (hash * 31) + (Model?.GetHashCode() ?? 0);
        hash = (hash * 31) + (UnsupportedInterfaceName?.GetHashCode() ?? 0);
        hash = (hash * 31) + (Location?.GetHashCode() ?? 0);
        return hash;
    }
}
