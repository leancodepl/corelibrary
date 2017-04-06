using System.Reflection;
using Autofac;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class RemoteHttpServerComponentTests
    {
        private static readonly Assembly ThisAssembly = typeof(RemoteHttpServerComponentTests).GetTypeInfo().Assembly;

        private readonly ContainerBuilder builder;

        public RemoteHttpServerComponentTests()
        {
            builder = new ContainerBuilder();
            builder.RegisterType<StubQueryExecutor>().AsImplementedInterfaces();
            builder.RegisterType<StubCommandExecutor>().AsImplementedInterfaces();
        }

        [Fact]
        public void Is_module_correct()
        {
            Register();
            builder.Build();
        }

        [Fact]
        public void Registers_query_executor_correctly()
        {
            Register();
            Assert.True(builder.Build().TryResolve<RemoteQueryHandler>(out var _));
        }

        [Fact]
        public void Register_command_executor_correctly()
        {
            Register();
            Assert.True(builder.Build().TryResolve<RemoteCommandHandler>(out var _));
        }

        private void Register(Assembly assembly = null)
        {
            var component = new RemoteHttpServerComponent(assembly ?? ThisAssembly);
            builder.RegisterModule(component.AutofacModule);
        }
    }
}
