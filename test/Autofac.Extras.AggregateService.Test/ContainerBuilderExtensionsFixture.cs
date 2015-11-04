using System;
using Moq;
using Xunit;

namespace Autofac.Extras.AggregateService.Test
{
    public class ContainerBuilderExtensionsFixture
    {
        [Fact]
        public void RegisterAggregateService_WithGeneric_RegistersServiceInterface()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<IMyContext>();
            var container = builder.Build();

            Assert.True(container.IsRegistered<IMyContext>());
        }

        [Fact]
        public void RegisterAggregateService_WithType_RegistersServiceInterface()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService(typeof(IMyContext));
            var container = builder.Build();

            Assert.True(container.IsRegistered<IMyContext>());
        }

        [Fact]
        public void RegisterAggregateService_WithNullInterfaceType_ThrowsArgumentNullException()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterAggregateService(null));
        }

        [Fact]
        public void RegisterAggregateService_WithNonInterfaceType_ThrowsArgumentException()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentException>(() => builder.RegisterAggregateService(typeof(MyServiceImpl)));
        }

        [Fact]
        public void RegisterAggregateService_WithGenericNonInterfaceType_ThrowsArgumentException()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentException>(() => builder.RegisterAggregateService<MyServiceImpl>());
        }

        [Fact]
        public void RegisterAggregateService_DifferentLifeTimeScopes_YieldsDifferentInstances()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService(typeof(IMyContext));
            builder.RegisterType<MyServiceImpl>()
                .As<IMyService>()
                .InstancePerLifetimeScope();
            var container = builder.Build();

            var rootScope = container.Resolve<IMyContext>();
            var subScope = container.BeginLifetimeScope().Resolve<IMyContext>();

            Assert.NotSame(subScope.MyService, rootScope.MyService);
        }

        [Fact]
        public void RegisterAggregateService_IsPerDependencyScoped()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAggregateService<IMyContext>();
            builder.RegisterInstance(new Mock<IMyService>().Object);
            var container = builder.Build();

            var firstInstance = container.Resolve<IMyContext>();
            var secondInstance = container.Resolve<IMyContext>();

            Assert.NotSame(secondInstance, firstInstance);
        }
    }
}