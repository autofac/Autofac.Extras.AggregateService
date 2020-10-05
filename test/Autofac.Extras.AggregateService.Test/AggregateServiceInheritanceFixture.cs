// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Moq;
using Xunit;

namespace Autofac.Extras.AggregateService.Test
{
    public class AggregateServiceInheritanceFixture
    {
        private readonly IContainer _container;

        private readonly ISubService _aggregateService;

        private readonly ISomeDependency _someDependencyMock;

        private readonly ISomeOtherDependency _someOtherDependencyMock;

        public AggregateServiceInheritanceFixture()
        {
            _someDependencyMock = new Mock<ISomeDependency>().Object;
            _someOtherDependencyMock = new Mock<ISomeOtherDependency>().Object;

            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<ISubService>();
            builder.RegisterInstance(_someDependencyMock);
            builder.RegisterInstance(_someOtherDependencyMock);
            _container = builder.Build();

            _aggregateService = _container.Resolve<ISubService>();
        }

        [Fact]
        public void Resolve_PropertyOnSuperType()
        {
            Assert.Equal(_someDependencyMock, _aggregateService.SomeDependency);
        }

        [Fact]
        public void Resolve_PropertyOnSubType()
        {
            Assert.Equal(_someOtherDependencyMock, _aggregateService.SomeOtherDependency);
        }
    }
}
