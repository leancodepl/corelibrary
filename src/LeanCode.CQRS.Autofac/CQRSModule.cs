using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.Variance;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Default.Security;
using LeanCode.CQRS.Security;

using Module = Autofac.Module;

namespace LeanCode.CQRS.Autofac
{
    class CQRSModule : Module
    {
        private readonly Assembly[] assemblies;

        public CQRSModule(Type[] searchAssemblies)
        {
            assemblies = searchAssemblies
                .Select(t => t.GetTypeInfo().Assembly)
                .ToArray();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new ContravariantRegistrationSource());

            builder.RegisterType<AutofacCommandHandlerResolver>().As<ICommandHandlerResolver>().SingleInstance();
            builder.RegisterType<AutofacQueryHandlerResolver>().As<IQueryHandlerResolver>().SingleInstance();

            builder.RegisterType<DefaultQueryExecutor>().As<IQueryExecutor>();
            builder.RegisterType<DefaultCommandExecutor>().As<ICommandExecutor>();
            builder.RegisterType<DefaultCqrs>().As<ICqrs>();
            builder.RegisterType<DefaultQueryCacheKeyProvider>().As<IQueryCacheKeyProvider>();
            builder.RegisterType<PositiveAuthorizationChecker>().As<IAuthorizationChecker>();

            builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(ICommandHandler<>));
            builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(IQueryHandler<,>));
        }
    }
}
