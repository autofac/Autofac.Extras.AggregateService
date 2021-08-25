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
        private readonly IContainer _containerClosedRegistrations;

        public AggregateServiceOpenGenericFixture()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<>));
            builder.RegisterGeneric(typeof(OpenGenericImpl<>))
                .As(typeof(IOpenGeneric<>));

            builder.RegisterInstance("Hello World!");
            builder.RegisterType<MyServiceImpl>().As<IMyService>();

            _container = builder.Build();

            // Closed registration to test for regression issues.
            var builderClosed = new ContainerBuilder();
            builderClosed.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<string>));
            builderClosed.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<IMyService>));
            builderClosed.RegisterGeneric(typeof(OpenGenericImpl<>))
                .As(typeof(IOpenGeneric<>));

            builderClosed.RegisterType<MyServiceImpl>().As<IMyService>();
            builderClosed.RegisterInstance("Hello World!");

            _containerClosedRegistrations = builderClosed.Build();
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

        [Fact]
        public void ResolvePropertyAsStringClosed()
        {
            var aggregateService = _containerClosedRegistrations.Resolve<IOpenGenericAggregateWithTypeParameter<string>>();

            var typeOfSomeProperty = aggregateService.SomeProperty.GetType();
            Assert.Same(typeof(string), typeOfSomeProperty);
            Assert.Same("Hello World!", aggregateService.SomeProperty);

            var generic = aggregateService.OpenGeneric;
            Assert.NotNull(generic);
            var typeOfOpenGeneric = generic.GetType().GetGenericArguments().Single();
            Assert.Same(typeof(string), typeOfOpenGeneric);
        }

        [Fact]
        public void ResolvePropertyAsMyServiceClosed()
        {
            var aggregateService = _containerClosedRegistrations.Resolve<IOpenGenericAggregateWithTypeParameter<IMyService>>();

            var typeOfSomeProperty = aggregateService.SomeProperty.GetType();
            Assert.Same(typeof(MyServiceImpl), typeOfSomeProperty);

            var generic = aggregateService.OpenGeneric;
            Assert.NotNull(generic);
            var typeOfOpenGeneric = generic.GetType().GetGenericArguments().Single();
            Assert.Same(typeof(IMyService), typeOfOpenGeneric);
        }

        [Fact]
        public void DeeplyNestedOpenGenericIsNotSupported()
        {
            var builder = new ContainerBuilder();

            // while
            // builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<IOpenGeneric<>>));
            // is not syntactical legal, the following however is.
            var myTrickyType = typeof(IOpenGenericAggregateWithTypeParameter<>).MakeGenericType(typeof(IOpenGeneric<>));

            var action = new Action(() => builder.RegisterAggregateService(myTrickyType));
            Assert.Throws<ArgumentException>(action);
        }
    }
}
