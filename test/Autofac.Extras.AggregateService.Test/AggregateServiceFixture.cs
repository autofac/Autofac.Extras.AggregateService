using System;
using Moq;
using Xunit;

namespace Autofac.Extras.AggregateService.Test
{
    public class AggregateServiceFixture
    {
        private IContainer _container;

        private ISomeDependency _someDependencyMock;

        private IMyContext _aggregateService;

        public AggregateServiceFixture()
        {
            _someDependencyMock = new Mock<ISomeDependency>().Object;

            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<IMyContext>();
            builder.RegisterType<MyServiceImpl>()
                .As<IMyService>()
                .InstancePerDependency();
            builder.RegisterInstance(_someDependencyMock);
            _container = builder.Build();

            _aggregateService = _container.Resolve<IMyContext>();
        }

        [Fact]
        public void Property_ResolvesService()
        {
            var service = _aggregateService.MyService;
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IMyService>(service);
        }

        [Fact]
        public void Property_Getter_AlwaysReturnSameInstance()
        {
            var firstInstance = _aggregateService.MyService;
            var secondInstance = _aggregateService.MyService;

            Assert.Same(secondInstance, firstInstance);
        }

        [Fact]
        public void Property_Setter_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _aggregateService.PropertyWithSetter = null);
        }

        [Fact]
        public void Method_WithVoid_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _aggregateService.MethodWithoutReturnValue());
        }

        [Fact]
        public void Method_ResolvesService()
        {
            var service = _aggregateService.GetMyService();
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IMyService>(service);
        }

        [Fact]
        public void Method_WithParameter_PassesParameterToService()
        {
            var myService = _aggregateService.GetMyService(10);

            Assert.Equal(10, myService.SomeIntValue);
        }

        [Fact]
        public void Method_WithParameters_PassesParametersToService()
        {
            var someDate = DateTime.Now;
            var myService = _aggregateService.GetMyService(someDate, 20);

            Assert.Equal(someDate, myService.SomeDateValue);
            Assert.Equal(20, myService.SomeIntValue);
        }

        [Fact]
        public void Method_WithNullParameters_PassesParametersToService()
        {
            var myService = _aggregateService.GetMyService(null);

            Assert.Null(myService.SomeStringValue);
        }

        [Fact]
        public void Method_WithParameter_PassesParameterAndOtherDependenciesToService()
        {
            var myService = _aggregateService.GetMyService("text");

            Assert.Equal("text", myService.SomeStringValue);
            Assert.Equal(_someDependencyMock, myService.SomeDependency);
        }
    }
}