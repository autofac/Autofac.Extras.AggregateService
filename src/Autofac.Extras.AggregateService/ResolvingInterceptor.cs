// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Castle.DynamicProxy;

namespace Autofac.Extras.AggregateService
{
    /// <summary>
    /// Interceptor that resolves types of properties and methods using a <see cref="IComponentContext"/>.
    /// </summary>
    public class ResolvingInterceptor : IInterceptor
    {
        private static readonly Assembly SystemAssembly = typeof(Func<>).Assembly;

        private readonly IComponentContext _context;

        private readonly Dictionary<MethodInfo, Action<IInvocation>> _invocationMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvingInterceptor"/> class.
        /// </summary>
        /// <param name="interfaceType">Type of the interface to intercept.</param>
        /// <param name="context">The resolution context.</param>
        public ResolvingInterceptor(Type interfaceType, IComponentContext context)
        {
            _context = context;
            _invocationMap = SetupInvocationMap(interfaceType);
        }

        /// <summary>
        /// Intercepts a method invocation.
        /// </summary>
        /// <param name="invocation">
        /// The method invocation to intercept.
        /// </param>
        public void Intercept(IInvocation invocation)
        {
            if (invocation == null)
            {
                throw new ArgumentNullException(nameof(invocation));
            }

            // Generic methods need to use the open generic method definition.
            var method = invocation.Method.IsGenericMethod ? invocation.Method.GetGenericMethodDefinition() : invocation.Method;
            var invocationHandler = _invocationMap[method];
            invocationHandler(invocation);
        }

        private static PropertyInfo? GetProperty(MethodInfo method)
        {
            var takesArg = method.GetParameters().Length == 1;
            var hasReturn = method.ReturnType != typeof(void);

            if (takesArg == hasReturn)
            {
                return null;
            }

            return method
                .DeclaringType
                .GetProperties()
                .Where(prop => prop.GetGetMethod() == method)
                .FirstOrDefault();
        }

        private static void InvalidReturnTypeInvocation(IInvocation invocation)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The method {0} has invalid return type System.Void", invocation.Method));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method gets called via reflection.")]
        [SuppressMessage("Microsoft.Performance", "IDE0051", Justification = "This method gets called via reflection.")]
        private void MethodWithoutParams(IInvocation invocation)
        {
            // To handle open generics, this resolves the return type of the invocation rather than the scanned method.
            invocation.ReturnValue = _context.Resolve(invocation.Method.ReturnType);
        }

        private Dictionary<MethodInfo, Action<IInvocation>> SetupInvocationMap(Type interfaceType)
        {
            var methods = interfaceType
                .GetUniqueInterfaces()
                .SelectMany(x => x.GetMethods())
                .ToArray();

            var methodMap = new Dictionary<MethodInfo, Action<IInvocation>>(methods.Length);
            foreach (var method in methods)
            {
                var returnType = method.ReturnType;

                if (returnType == typeof(void))
                {
                    // Any method with 'void' return type (includes property setters) should throw an exception
                    methodMap.Add(method, InvalidReturnTypeInvocation);
                    continue;
                }

                if (GetProperty(method) != null)
                {
                    // All properties should be resolved at proxy instantiation
                    var propertyValue = _context.Resolve(returnType);
                    methodMap.Add(method, invocation => invocation.ReturnValue = propertyValue);
                    continue;
                }

                // For methods with parameters, cache parameter info for use at invocation time
                var parameters = method.GetParameters()
                    .OrderBy(parameterInfo => parameterInfo.Position)
                    .Select(parameterInfo => new { parameterInfo.Position, parameterInfo.ParameterType })
                    .ToArray();

                if (parameters.Length > 0)
                {
                    // Methods with parameters
                    if (parameters.Any(p => p.ParameterType.IsGenericType))
                    {
                        // There are some open generic parameters so resolve a
                        // Func<X, Y> corresponding to the method parameters and
                        // method return type. Core Autofac will handle the type
                        // mapping from open generic to closed generic, etc.
                        methodMap.Add(
                            method,
                            invocation =>
                            {
                                var targetMethod = invocation.Method;
                                var funcArgTypes = targetMethod.GetParameters().OrderBy(p => p.Position).Select(p => p.ParameterType).Append(targetMethod.ReturnType).ToArray();
                                var funcTypeName = $"System.Func`{funcArgTypes.Length}";
                                var baseFuncType = SystemAssembly.GetType(funcTypeName);
                                if (baseFuncType == null)
                                {
                                    throw new NotSupportedException($"Unable to locate function type for dynamic resolution: {funcTypeName}. Ensure your method doesn't have too many parameters to convert to a System.Func delegate.");
                                }

                                var builtFuncType = baseFuncType.MakeGenericType(funcArgTypes);
                                var factory = (Delegate)_context.Resolve(builtFuncType);
                                invocation.ReturnValue = factory!.DynamicInvoke(invocation.Arguments);
                            });
                    }
                    else
                    {
                        // There are no open generic parameters so we can simplify the backing method.
                        methodMap.Add(
                            method,
                            invocation =>
                            {
                                var arguments = invocation.Arguments;
                                var typedParameters = parameters
                                    .Select(info => (Parameter)new TypedParameter(info.ParameterType, arguments[info.Position]));

                                // To handle open generics, this resolves the return type of the invocation rather than the scanned method.
                                invocation.ReturnValue = _context.Resolve(invocation.Method.ReturnType, typedParameters);
                            });
                    }

                    continue;
                }

                // Methods without parameters
                var methodWithoutParams = GetType().GetMethod("MethodWithoutParams", BindingFlags.Instance | BindingFlags.NonPublic);
                var methodWithoutParamsDelegate = (Action<IInvocation>)methodWithoutParams.CreateDelegate(typeof(Action<IInvocation>), this);
                methodMap.Add(method, methodWithoutParamsDelegate);
            }

            return methodMap;
        }
    }
}
