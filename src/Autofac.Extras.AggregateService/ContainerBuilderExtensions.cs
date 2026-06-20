// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Autofac.Extras.AggregateService;

/// <summary>
/// AggregateService extensions to <see cref="ContainerBuilder"/>.
/// </summary>
public static class ContainerBuilderExtensions
{
    /// <summary>
    /// Register <typeparamref name="TInterface"/> as an aggregate service.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <typeparam name="TInterface">The interface type to register.</typeparam>
    /// <exception cref="ArgumentNullException">If <typeparamref name="TInterface"/> is null.</exception>
    /// <exception cref="ArgumentException">If <typeparamref name="TInterface"/> is not an interface.</exception>
    public static void RegisterAggregateService<TInterface>(this ContainerBuilder builder)
        where TInterface : class
    {
        builder.RegisterAggregateService(typeof(TInterface));
    }

    /// <summary>
    /// Register <paramref name="interfaceType"/> as an aggregate service.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <param name="interfaceType">The interface type to register.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="interfaceType"/> is null.</exception>
    /// <exception cref="ArgumentException">If <paramref name="interfaceType"/> is not an interface.</exception>
    public static void RegisterAggregateService(this ContainerBuilder builder, Type interfaceType)
    {
        if (interfaceType == null)
        {
            throw new ArgumentNullException(nameof(interfaceType));
        }

        if (!interfaceType.GetTypeInfo().IsInterface)
        {
            throw new ArgumentException(AggregateServicesResources.TypeMustBeInterface, nameof(interfaceType));
        }

        if (interfaceType.IsGenericTypeDefinition)
        {
            RegisterAggregateServiceAsOpenGeneric(builder, interfaceType);
        }
        else
        {
            if (interfaceType.ContainsGenericParameters)
            {
                throw new ArgumentException(AggregateServicesResources.InterfaceMayNotContainGenericParameters, nameof(interfaceType));
            }

            RegisterAggregateServiceAsInstance(builder, interfaceType);
        }
    }

    private static void RegisterAggregateServiceAsInstance(ContainerBuilder builder, Type interfaceType)
        => builder.Register(c =>
                AggregateServiceGenerator.CreateInstance(interfaceType, c.Resolve<IComponentContext>()))
            .As(interfaceType)
            .InstancePerDependency();

    // Registering an open generic aggregate service constructs the closed interface type at
    // resolution time (MakeGenericType), which is inherently a dynamic-code operation. This is
    // only reached for open generic definitions (gated by IsGenericTypeDefinition in the caller),
    // so the common closed-interface registration path stays AOT/trim-clean. Open-generic
    // registration under NativeAOT is documented as an advanced, not-fully-AOT-safe scenario.
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Open generic aggregate service registration requires MakeGenericType. Only reached for open generic definitions; closed registrations do not hit this path. Documented as not fully AOT-safe.")]
    [UnconditionalSuppressMessage("Trimming", "IL2055", Justification = "See IL3050 justification.")]
    private static void RegisterAggregateServiceAsOpenGeneric(ContainerBuilder builder, Type interfaceType)
        => builder.RegisterGeneric((c, types) =>
                AggregateServiceGenerator.CreateInstance(
                    interfaceType.MakeGenericType(types),
                    c.Resolve<IComponentContext>()))
            .As(interfaceType)
            .InstancePerDependency();
}
