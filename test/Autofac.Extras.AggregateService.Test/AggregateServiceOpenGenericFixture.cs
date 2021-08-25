// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Autofac.Extras.AggregateService.Test
{
    public class AggregateServiceOpenGenericFixture
    {
        private readonly IContainer _container;

        public AggregateServiceOpenGenericFixture()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<>));
            builder.RegisterGeneric(typeof(OpenGenericImpl<>))
                .As(typeof(IOpenGeneric<>));

            builder.RegisterInstance("Hello World!");
            builder.RegisterType<MyServiceImpl>().As<IMyService>();

            _container = builder.Build();
        }

        [Fact]
        public void ResolvePropertyAsString()
        {
            var aggregateService = _container.Resolve<IOpenGenericAggregateWithTypeParameter<string>>();

            var typeOfSomeProperty = aggregateService.SomeProperty.GetType();
            Assert.Same(typeof(string), typeOfSomeProperty);
            Assert.Same("Hello World!", aggregateService.SomeProperty);

            var generic = aggregateService.OpenGeneric;
            Assert.NotNull(generic);
            var typeOfOpenGeneric = generic.GetType().GetGenericArguments().Single();
            Assert.Same(typeof(string), typeOfOpenGeneric);
        }

        /// <summary>
        ///
        /// Attempts to resolve a property of the given type.
        /// </summary>
        [Fact]
        public void ResolvePropertyAsMyService()
        {
            var aggregateService = _container.Resolve<IOpenGenericAggregateWithTypeParameter<IMyService>>();

            var typeOfSomeProperty = aggregateService.SomeProperty.GetType();
            Assert.Same(typeof(MyServiceImpl), typeOfSomeProperty);

            var generic = aggregateService.OpenGeneric;
            Assert.NotNull(generic);
            var typeOfOpenGeneric = generic.GetType().GetGenericArguments().Single();
            Assert.Same(typeof(IMyService), typeOfOpenGeneric);
        }
    }
}
