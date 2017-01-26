// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2010 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Castle.DynamicProxy;

namespace Autofac.Extras.AggregateService
{
    /// <summary>
    /// Interceptor that resolves types of properties and methods using a <see cref="IComponentContext"/>.
    /// </summary>
    public class ResolvingInterceptor : IInterceptor
    {
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
                throw new ArgumentNullException("invocation");
            }

            // Generic methods need to use the open generic method definition.
            var method = invocation.Method.IsGenericMethod ? invocation.Method.GetGenericMethodDefinition() : invocation.Method;
            var invocationHandler = _invocationMap[method];
            invocationHandler(invocation);
        }

        private static PropertyInfo GetProperty(MethodInfo method)
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
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "The method {0} has invalid return type System.Void", invocation.Method));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method gets called via reflection.")]
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

            var methodMap = new Dictionary<MethodInfo, Action<IInvocation>>(methods.Count());
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
                    methodMap.Add(method, invocation =>
                        {
                            var arguments = invocation.Arguments;
                            var typedParameters = parameters
                                .Select(info => (Parameter)new TypedParameter(info.ParameterType, arguments[info.Position]));

                            // To handle open generics, this resolves the return type of the invocation rather than the scanned method.
                            invocation.ReturnValue = _context.Resolve(invocation.Method.ReturnType, typedParameters);
                        });

                    continue;
                }

                // Methods without parameters
                var methodWithoutParams = this.GetType().GetMethod("MethodWithoutParams", BindingFlags.Instance | BindingFlags.NonPublic);
                var methodWithoutParamsDelegate = (Action<IInvocation>)methodWithoutParams.CreateDelegate(typeof(Action<IInvocation>), this);
                methodMap.Add(method, methodWithoutParamsDelegate);
            }

            return methodMap;
        }
    }
}