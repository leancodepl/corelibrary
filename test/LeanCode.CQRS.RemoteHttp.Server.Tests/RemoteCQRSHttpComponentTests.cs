using System;
using Autofac;
using Autofac.Features.Indexed;
using LeanCode.Components;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class RemoteHttpServerComponentTests
    {
        private static TypesCatalog ThisCatalog => new TypesCatalog(typeof(RemoteHttpServerComponentTests));
        private static TypesCatalog OtherCatalog => new TypesCatalog(typeof(String));

        private readonly ContainerBuilder builder;

        public RemoteHttpServerComponentTests()
        {
            builder = new ContainerBuilder();
            builder.RegisterType<StubQueryExecutor>().AsImplementedInterfaces();
            builder.RegisterType<StubCommandExecutor>().AsImplementedInterfaces();
        }

        [Fact]
        public void Module_is_correct()
        {
            Register();
            builder.Build();
        }

        [Fact]
        public void Registers_query_executor_correctly()
        {
            Register();

            Assert.True(builder.Build().TryResolve<IIndex<TypesCatalog, RemoteQueryHandler>>(out var factory));

            var qh = factory[ThisCatalog];
            Assert.Equal(ThisCatalog, qh.Catalog);
        }

        [Fact]
        public void Register_command_executor_correctly()
        {
            Register();

            Assert.True(builder.Build().TryResolve<IIndex<TypesCatalog, RemoteCommandHandler>>(out var factory));

            var ch = factory[ThisCatalog];
            Assert.Equal(ThisCatalog, ch.Catalog);
        }

        [Fact]
        public void Returns_different_query_handlers_for_different_assemblies()
        {
            Register();
            var factory = builder.Build().Resolve<IIndex<TypesCatalog, RemoteQueryHandler>>();

            var qh1 = factory[ThisCatalog];
            var qh2 = factory[OtherCatalog];

            Assert.NotSame(qh1, qh2);
        }

        [Fact]
        public void Returns_the_same_query_handlers_for_the_same_assemblies()
        {
            Register();
            var factory = builder.Build().Resolve<IIndex<TypesCatalog, RemoteQueryHandler>>();

            var qh1 = factory[ThisCatalog];
            var qh2 = factory[ThisCatalog];

            Assert.Same(qh1, qh2);
        }

        [Fact]
        public void Returns_different_command_handlers_for_different_assemblies()
        {
            Register();
            var factory = builder.Build().Resolve<IIndex<TypesCatalog, RemoteCommandHandler>>();

            var qh1 = factory[ThisCatalog];
            var qh2 = factory[OtherCatalog];

            Assert.NotSame(qh1, qh2);
        }

        [Fact]
        public void Returns_the_same_command_handlers_for_the_same_assemblies()
        {
            Register();
            var factory = builder.Build().Resolve<IIndex<TypesCatalog, RemoteCommandHandler>>();

            var qh1 = factory[ThisCatalog];
            var qh2 = factory[ThisCatalog];

            Assert.Same(qh1, qh2);
        }

        private void Register()
        {
            var component1 = new RemoteCQRSHttpComponent(ThisCatalog);
            builder.RegisterModule(component1.AutofacModule);

            var component2 = new RemoteCQRSHttpComponent(OtherCatalog);
            builder.RegisterModule(component2.AutofacModule);
        }
    }
}
