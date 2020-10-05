// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Autofac.Extras.AggregateService
{
    /// <summary>
    /// Extension methods for working with <see cref="System.Type"/>.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Return unique interfaces implemented or inherited by <paramref name="type"/>.
        /// Will also include <paramref name="type"/> if it is an interface type.
        /// </summary>
        /// <param name="type">
        /// The type for which interfaces should be retrieved.
        /// </param>
        /// <returns>
        /// A list of unique interfaces implemented by the provided type.
        /// </returns>
        public static IEnumerable<Type> GetUniqueInterfaces(this Type type)
        {
            var types = new HashSet<Type>();
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (types.Contains(interfaceType))
                {
                    continue;
                }

                types.Add(interfaceType);
            }

            if (type.GetTypeInfo().IsInterface && !types.Contains(type))
            {
                types.Add(type);
            }

            return types;
        }
    }
}
