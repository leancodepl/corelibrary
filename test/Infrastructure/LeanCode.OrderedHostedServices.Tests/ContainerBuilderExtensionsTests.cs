using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.OrderedHostedServices.Tests
{
    public class ContainerBuilderExtensionsTests
    {
        [Fact]
        public void Cannot_add_ordered_service_as_normal_service()
        {
            Assert.Throws<ArgumentException>(() => Build(b => b.AddHostedService<Ordered>()));
        }

        [Fact]
        public void Cannot_add_ordered_service_as_wrapped_normal_service()
        {
            Assert.Throws<ArgumentException>(() => Build(b => b.AddOrderedHostedService<Ordered>(0)));
        }

        [Fact]
        public void Ordered_service_is_resolvable_only_as_an_IOrderedHostedService_interface()
        {
            var container = Build(b => b.AddOrderedHostedService<Ordered>());

            Assert.IsType<Ordered>(container.Resolve<IOrderedHostedService>());
            Assert.Null(container.ResolveOptional<IHostedService>());
            Assert.Null(container.ResolveOptional<Ordered>());
        }

        [Fact]
        public void Wrapped_service_is_registered_as_self_and_as_an_IOrderedHostedService()
        {
            var container = Build(b => b.AddOrderedHostedService<Normal>(0));

            Assert.IsType<HostedServiceWrapper<Normal>>(container.Resolve<IOrderedHostedService>());
            Assert.IsType<Normal>(container.ResolveOptional<Normal>());
            Assert.Null(container.ResolveOptional<IHostedService>());
        }

        [Fact]
        public void Normal_service_is_resolvable_only_as_an_IHostedService_interface()
        {
            var container = Build(b => b.AddHostedService<Normal>());

            Assert.IsType<Normal>(container.Resolve<IHostedService>());
            Assert.Null(container.ResolveOptional<IOrderedHostedService>());
            Assert.Null(container.ResolveOptional<Normal>());
        }

        private static IContainer Build(Action<ContainerBuilder> cfg)
        {
            var builder = new ContainerBuilder();
            cfg(builder);
            return builder.Build();
        }

        private class Ordered : IOrderedHostedService
        {
            public int Order => throw new NotImplementedException();
            public Task StartAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task StopAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        }

        private class Normal : IHostedService
        {
            public Task StartAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task StopAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        }
    }
}
