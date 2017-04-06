using System;
using System.Reflection;
using Autofac;
using Autofac.Features.Indexed;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class RemoteHttpServerComponentTests
    {
        private static readonly Assembly ThisAssembly = typeof(RemoteHttpServerComponentTests).GetTypeInfo().Assembly;
        private static readonly Assembly OtherAssembly = typeof(String).GetTypeInfo().Assembly;

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

            Assert.True(builder.Build().TryResolve<IIndex<Assembly, RemoteQueryHandler>>(out var factory));

            var qh = factory[ThisAssembly];
            Assert.Same(ThisAssembly, qh.TypesAssembly);
        }

        [Fact]
        public void Register_command_executor_correctly()
        {
            Register();

            Assert.True(builder.Build().TryResolve<IIndex<Assembly, RemoteCommandHandler>>(out var factory));

            var ch = factory[ThisAssembly];
            Assert.Same(ThisAssembly, ch.TypesAssembly);
        }

        [Fact]
        public void Returns_different_query_handlers_for_different_assemblies()
        {
            Register();
            var factory = builder.Build().Resolve<IIndex<Assembly, RemoteQueryHandler>>();

            var qh1 = factory[ThisAssembly];
            var qh2 = factory[OtherAssembly];

            Assert.NotSame(qh1, qh2);
        }

        [Fact]
        public void Returns_the_same_query_handlers_for_the_same_assemblies()
        {
            Register();
            var factory = builder.Build().Resolve<IIndex<Assembly, RemoteQueryHandler>>();

            var qh1 = factory[ThisAssembly];
            var qh2 = factory[ThisAssembly];

            Assert.Same(qh1, qh2);
        }

        [Fact]
        public void Returns_different_command_handlers_for_different_assemblies()
        {
            Register();
            var factory = builder.Build().Resolve<IIndex<Assembly, RemoteCommandHandler>>();

            var qh1 = factory[ThisAssembly];
            var qh2 = factory[OtherAssembly];

            Assert.NotSame(qh1, qh2);
        }

        [Fact]
        public void Returns_the_same_command_handlers_for_the_same_assemblies()
        {
            Register();
            var factory = builder.Build().Resolve<IIndex<Assembly, RemoteCommandHandler>>();

            var qh1 = factory[ThisAssembly];
            var qh2 = factory[ThisAssembly];

            Assert.Same(qh1, qh2);
        }

        private void Register()
        {
            var component1 = new RemoteCQRSHttpComponent(ThisAssembly);
            builder.RegisterModule(component1.AutofacModule);

            var component2 = new RemoteCQRSHttpComponent(OtherAssembly);
            builder.RegisterModule(component2.AutofacModule);
        }
    }
}
