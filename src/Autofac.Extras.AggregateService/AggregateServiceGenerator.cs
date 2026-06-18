// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Castle.DynamicProxy;

namespace Autofac.Extras.AggregateService;

/// <summary>
/// Generate aggregate service instances from interface types.
/// </summary>
public static class AggregateServiceGenerator
{
    private static readonly ProxyGenerator _generator = new ProxyGenerator();

    /// <summary>
    /// Generate an aggregate service instance that will resolve its types from <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The component context from where types will be resolved.</param>
    /// <typeparam name="TAggregateServiceInterface">The interface type for the aggregate service.</typeparam>
    /// <returns>The aggregate service instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <typeparamref name="TAggregateServiceInterface"/> is not an interface.</exception>
    public static object CreateInstance<TAggregateServiceInterface>(IComponentContext context)
    {
        return CreateInstance(typeof(TAggregateServiceInterface), context);
    }

    /// <summary>
    /// Generate an aggregate service instance that will resolve its types from <paramref name="context"/>.
    /// </summary>
    /// <param name="interfaceType">The interface type for the aggregate service.</param>
    /// <param name="context">The component context from where types will be resolved.</param>
    /// <returns>The aggregate service instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="interfaceType"/> is not an interface.</exception>
    public static object CreateInstance(Type interfaceType, IComponentContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (interfaceType == null)
        {
            throw new ArgumentNullException(nameof(interfaceType));
        }

        if (!interfaceType.GetTypeInfo().IsInterface)
        {
            throw new ArgumentException(AggregateServicesResources.TypeMustBeInterface, paramName: nameof(interfaceType));
        }

        // Prefer a source-generated implementation when one is available. This avoids the
        // dynamic proxy entirely and is trimming/NativeAOT safe. Falls back to Castle
        // DynamicProxy when the generator could not see this interface statically.
        if (GeneratedAggregateServiceRegistry.TryCreate(interfaceType, context, out var generated))
        {
            return generated;
        }

        return CreateProxyInstance(interfaceType, context);
    }

    [RequiresUnreferencedCode("The dynamic proxy fallback uses reflection over the aggregate service interface and is not compatible with trimming. Aggregate services discovered statically by the source generator do not use this path.")]
    [RequiresDynamicCode("The dynamic proxy fallback emits IL at runtime and is not compatible with NativeAOT. Aggregate services discovered statically by the source generator do not use this path.")]
    private static object CreateProxyInstance(Type interfaceType, IComponentContext context)
    {
        var resolverInterceptor = new ResolvingInterceptor(interfaceType, context);
        return _generator.CreateInterfaceProxyWithoutTarget(interfaceType, resolverInterceptor);
    }
}
