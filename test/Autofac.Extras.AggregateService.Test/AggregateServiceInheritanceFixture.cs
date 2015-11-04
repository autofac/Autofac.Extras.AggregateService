using Moq;
using Xunit;

namespace Autofac.Extras.AggregateService.Test
{
    public class AggregateServiceInheritanceFixture
    {
        private IContainer _container;

        private ISubService _aggregateService;

        private ISomeDependency _someDependencyMock;

        private ISomeOtherDependency _someOtherDependencyMock;

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
