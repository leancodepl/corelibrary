using Autofac;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    class RemoteHttpModule : Autofac.Module
    {
        private readonly TypesCatalog catalog;

        public RemoteHttpModule(TypesCatalog catalog)
        {
            this.catalog = catalog;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RemoteCommandHandler>()
                .Keyed<RemoteCommandHandler>(catalog).WithParameter(nameof(catalog), catalog)
                .SingleInstance();

            builder.RegisterType<RemoteQueryHandler>()
                .Keyed<RemoteQueryHandler>(catalog).WithParameter(nameof(catalog), catalog)
                .SingleInstance();
        }
    }
}
