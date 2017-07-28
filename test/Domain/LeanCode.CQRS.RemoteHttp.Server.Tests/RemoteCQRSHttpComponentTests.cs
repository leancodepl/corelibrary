using Autofac;
using LeanCode.Components;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class RemoteHttpServerComponentTests
    {
        private static TypesCatalog ThisCatalog => new TypesCatalog(typeof(RemoteHttpServerComponentTests));

        private readonly ContainerBuilder builder;

        public RemoteHttpServerComponentTests()
        {
            builder = new ContainerBuilder();
            builder.RegisterType<StubQueryExecutor>().AsImplementedInterfaces();
            builder.RegisterType<StubCommandExecutor>().AsImplementedInterfaces();

            var component1 = new RemoteCQRSHttpComponent<AppContext>(ThisCatalog, ctx => new AppContext(ctx.User));
            builder.RegisterModule(component1.AutofacModule);
        }

        [Fact]
        public void Module_is_correct()
        {
            builder.Build();
        }

        [Fact]
        public void Registers_query_executor_correctly()
        {
            Assert.True(builder.Build().TryResolve<IRemoteQueryHandler>(out var qh));

            Assert.Equal(ThisCatalog, qh.Catalog);
        }

        [Fact]
        public void Register_command_executor_correctly()
        {
            Assert.True(builder.Build().TryResolve<IRemoteCommandHandler>(out var ch));

            Assert.Equal(ThisCatalog, ch.Catalog);
        }
    }
}
