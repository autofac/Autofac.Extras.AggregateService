// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Xunit;

namespace Autofac.Extras.AggregateService.Test
{
    public class AggregateServiceGenericsFixture
    {
        private readonly IContainer _container;

        public AggregateServiceGenericsFixture()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<IOpenGenericAggregate>();
            builder.RegisterGeneric(typeof(OpenGenericImpl<>))
                .As(typeof(IOpenGeneric<>));

            _container = builder.Build();
        }

        /// <summary>
        /// Attempts to resolve an open generic by a method call.
        /// </summary>
        [Fact]
        public void Method_ResolveOpenGeneric()
        {
            var aggregateService = _container.Resolve<IOpenGenericAggregate>();

            var generic = aggregateService.GetOpenGeneric<object>();
            Assert.NotNull(generic);

            var ungeneric = aggregateService.GetResolvedGeneric();
            Assert.NotNull(ungeneric);
            Assert.NotSame(generic, ungeneric);
        }
    }
}
