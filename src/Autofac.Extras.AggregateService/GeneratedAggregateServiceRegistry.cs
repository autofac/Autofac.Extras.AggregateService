// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Autofac.Extras.AggregateService;

/// <summary>
/// Registry of source-generated aggregate service implementations.
/// </summary>
/// <remarks>
/// <para>
/// The <c>Autofac.Extras.AggregateService.SourceGenerator</c> analyzer that
/// ships in this package emits a concrete backing class for each aggregate
/// service interface it can discover statically, along with a module
/// initializer that registers a factory here when the consuming assembly loads.
/// </para>
/// <para>
/// At resolution time
/// <see cref="AggregateServiceGenerator.CreateInstance(Type, IComponentContext)"/>
/// consults this registry first. When a generated implementation is available
/// it is used directly - no dynamic proxy, no per-invocation reflection - which
/// is also trimming and NativeAOT safe. When no generated implementation exists
/// (for example, an interface supplied via a runtime-computed
/// <see cref="Type"/> that the generator could not see), resolution falls back
/// to the Castle DynamicProxy implementation.
/// </para>
/// <para>
/// This type is part of the public API only so that generated code can call
/// into it. It is not intended to be called directly by application code.
/// </para>
/// </remarks>
public static class GeneratedAggregateServiceRegistry
{
    private static readonly object _syncRoot = new object();

    // Closed (non-generic, or already-closed generic) interface type -> factory.
    private static readonly Dictionary<Type, Func<IComponentContext, object>> _closedFactories =
        new Dictionary<Type, Func<IComponentContext, object>>();

    // Open generic interface definition -> open generic backing class definition.
    private static readonly Dictionary<Type, Type> _openGenericBackings = new Dictionary<Type, Type>();

    /// <summary>
    /// Registers a generated factory for a closed aggregate service interface
    /// type.
    /// </summary>
    /// <param name="interfaceType">
    /// The aggregate service interface the factory produces.
    /// </param>
    /// <param name="factory">
    /// A factory that builds the generated implementation from a component
    /// context.
    /// </param>
    /// <remarks>
    /// This is invoked by source-generated module initializers and is not
    /// intended for direct use.
    /// </remarks>
    public static void Register(Type interfaceType, Func<IComponentContext, object> factory)
    {
        if (interfaceType == null)
        {
            throw new ArgumentNullException(nameof(interfaceType));
        }

        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        lock (_syncRoot)
        {
            _closedFactories[interfaceType] = factory;
        }
    }

    /// <summary>
    /// Registers a generated open generic backing class for an open generic
    /// aggregate service interface.
    /// </summary>
    /// <param name="openInterfaceType">
    /// The open generic interface definition (for example,
    /// <c>typeof(IThing&lt;&gt;)</c>).
    /// </param>
    /// <param name="openBackingType">
    /// The open generic backing class definition that implements it.
    /// </param>
    /// <remarks>
    /// This is invoked by source-generated module initializers and is not
    /// intended for direct use.
    /// </remarks>
    public static void RegisterOpenGeneric(Type openInterfaceType, Type openBackingType)
    {
        if (openInterfaceType == null)
        {
            throw new ArgumentNullException(nameof(openInterfaceType));
        }

        if (openBackingType == null)
        {
            throw new ArgumentNullException(nameof(openBackingType));
        }

        lock (_syncRoot)
        {
            _openGenericBackings[openInterfaceType] = openBackingType;
        }
    }

    /// <summary>
    /// Attempts to create a generated aggregate service implementation for the
    /// given interface.
    /// </summary>
    /// <param name="interfaceType">
    /// The (closed) aggregate service interface type to create.
    /// </param>
    /// <param name="context">
    /// The component context used to resolve the aggregated dependencies.
    /// </param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, the generated
    /// implementation; otherwise <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a generated implementation was created;
    /// otherwise <see langword="false"/>.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The RequiresUnreferencedCode call (CreateFromOpenGenericBacking) is only reached when an open generic backing is registered, which itself only happens for open generic aggregate services. Closed aggregate services - the AOT-supported path - never reach it.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "The RequiresDynamicCode call (CreateFromOpenGenericBacking) is only reached for open generic aggregate services. Closed aggregate services - the AOT-supported path - never reach it.")]
    internal static bool TryCreate(Type interfaceType, IComponentContext context, [NotNullWhen(true)] out object? instance)
    {
        Func<IComponentContext, object>? factory = null;
        Type? openBacking = null;

        lock (_syncRoot)
        {
            if (!_closedFactories.TryGetValue(interfaceType, out factory) && interfaceType.IsConstructedGenericType)
            {
                _openGenericBackings.TryGetValue(interfaceType.GetGenericTypeDefinition(), out openBacking);
            }
        }

        // Closed (non-generic, or already-closed-generic-registered) aggregate services resolve
        // here through a statically-known factory. This is the common path and is fully
        // trimming/NativeAOT safe.
        if (factory != null)
        {
            instance = factory(context);
            return true;
        }

        // Open generic aggregate services must construct the closed backing type at runtime,
        // which is not NativeAOT-compatible (see CreateFromOpenGenericBacking).
        if (openBacking != null)
        {
            instance = CreateFromOpenGenericBacking(openBacking, interfaceType, context);
            return true;
        }

        instance = null;
        return false;
    }

    // Constructs and instantiates the closed backing type for an open generic aggregate service.
    // This is inherently NOT trimming/NativeAOT safe: the closed construction is determined at
    // runtime (MakeGenericType), and its constructor metadata is not statically discoverable, so
    // under NativeAOT this throws (the generic instantiation / constructor is trimmed away). Open
    // generic aggregate services are therefore a JIT-only feature - documented as not AOT-safe.
    // The closed-aggregate path above never calls this, so the common case remains clean.
    [RequiresUnreferencedCode("Open generic aggregate services construct their closed backing type at runtime and are not compatible with trimming. Register closed aggregate service interfaces for trimming/NativeAOT support.")]
    [RequiresDynamicCode("Open generic aggregate services use MakeGenericType to construct the closed backing type at runtime and are not compatible with NativeAOT. Register closed aggregate service interfaces for NativeAOT support.")]
    private static object CreateFromOpenGenericBacking(Type openBacking, Type interfaceType, IComponentContext context)
    {
        var closedBacking = openBacking.MakeGenericType(interfaceType.GenericTypeArguments);
        return Activator.CreateInstance(closedBacking, context)!;
    }
}
