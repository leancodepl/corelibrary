using Autofac;
using Autofac.Features.Variance;
using LeanCode.Components;
using LeanCode.CQRS.Autofac.Security;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Default.Security;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Autofac
{
    class CQRSModule : Module
    {
        private readonly TypesCatalog catalog;

        public CQRSModule(TypesCatalog catalog)
        {
            this.catalog = catalog;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new ContravariantRegistrationSource());

            builder.RegisterType<AutofacCommandHandlerResolver>().As<ICommandHandlerResolver>();
            builder.RegisterType<AutofacQueryHandlerResolver>().As<IQueryHandlerResolver>();
            builder.RegisterType<AutofacAuthorizerResolver>().As<IAuthorizerResolver>();

            builder.RegisterType<DefaultQueryExecutor>().As<IQueryExecutor>();
            builder.RegisterType<DefaultCommandExecutor>().As<ICommandExecutor>();
            builder.RegisterType<DefaultCqrs>().As<ICqrs>();
            builder.RegisterType<DefaultQueryCacheKeyProvider>().As<IQueryCacheKeyProvider>();
            builder.RegisterType<DefaultAuthorizer>().As<IAuthorizer>();
            builder.RegisterType<DefaultPermissionAuthorizer>().As<PermissionAuthorizer>();

            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(ICommandHandler<>));
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IQueryHandler<,>));
        }
    }
}
