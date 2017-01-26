using System;
using System.Collections.Generic;
using System.Reflection;

namespace Autofac.Extras.AggregateService
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Return unique interfaces implemented or inherited by <paramref name="type"/>.
        /// Will also include <paramref name="type"/> if it is an interface type.
        /// </summary>
        /// <param name="type">
        /// The type for which interfaces should be retrieved.
        /// </param>
        public static IEnumerable<Type> GetUniqueInterfaces(this Type type)
        {
            var types = new HashSet<Type>();
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (types.Contains(interfaceType))
                    continue;
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