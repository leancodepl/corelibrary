using System.Reflection;
using Autofac;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    class RemoteHttpModule : Autofac.Module
    {
        private readonly Assembly typesAssembly;

        public RemoteHttpModule(Assembly typesAssembly)
        {
            this.typesAssembly = typesAssembly;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RemoteCommandHandler>()
                .Keyed<RemoteCommandHandler>(typesAssembly).WithParameter(nameof(typesAssembly), typesAssembly)
                .SingleInstance();

            builder.RegisterType<RemoteQueryHandler>()
                .Keyed<RemoteQueryHandler>(typesAssembly).WithParameter(nameof(typesAssembly), typesAssembly)
                .SingleInstance();
        }
    }
}
