// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Xunit;

namespace Autofac.Extras.AggregateService.Test
{
    public class AggregateServiceOpenGenericRegressionFixture
    {
        private readonly IContainer _container;

        public AggregateServiceOpenGenericRegressionFixture()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<string>));
            builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<IMyService>));
            builder.RegisterGeneric(typeof(OpenGenericImpl<>))
                .As(typeof(IOpenGeneric<>));

            builder.RegisterType<MyServiceImpl>().As<IMyService>();
            builder.RegisterInstance("Hello World!");

            _container = builder.Build();
        }

        [Fact]
        public void ResolveProperty()
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
    }
}
