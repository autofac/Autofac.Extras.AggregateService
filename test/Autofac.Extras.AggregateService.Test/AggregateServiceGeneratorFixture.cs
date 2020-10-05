// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Moq;
using Xunit;

namespace Autofac.Extras.AggregateService.Test
{
    public class AggregateServiceGeneratorFixture
    {
        private readonly IContainer _container;

        public AggregateServiceGeneratorFixture()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new Mock<IMyService>().Object);
            _container = builder.Build();
        }

        [Fact]
        public void CreateInstance_WithGenericInterface_CreatesInstanceOfInterface()
        {
            var instance = AggregateServiceGenerator.CreateInstance<IMyContext>(_container);

            Assert.IsAssignableFrom<IMyContext>(instance);
        }

        [Fact]
        public void CreateInstance_WithInterfaceType_CreatesInstanceOfInterface()
        {
            var instance = AggregateServiceGenerator.CreateInstance(typeof(IMyContext), _container);

            Assert.IsAssignableFrom<IMyContext>(instance);
        }

        [Fact]
        public void CreateInstance_ExpectsInterfaceTypeInstance()
        {
            Assert.Throws<ArgumentNullException>(() => AggregateServiceGenerator.CreateInstance(null, _container));
        }

        [Fact]
        public void CreateInstance_ExpectsComponentInstance()
        {
            Assert.Throws<ArgumentNullException>(() => AggregateServiceGenerator.CreateInstance(typeof(IMyContext), null));
        }

        [Fact]
        public void CreateInstance_ExpectsInterfaceType()
        {
            Assert.Throws<ArgumentException>(() => AggregateServiceGenerator.CreateInstance<string>(_container));
        }
    }
}
