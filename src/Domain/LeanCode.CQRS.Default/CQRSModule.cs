using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Features.Variance;
using LeanCode.Components;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Default.Autofac;
using LeanCode.CQRS.Default.Execution;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.EventsExecution.Simple;
using LeanCode.Pipelines;
using LeanCode.Pipelines.Autofac;

namespace LeanCode.CQRS.Default
{
    using CommandsQueriesModules = IReadOnlyList<IModule>;

    class CQRSModule : Module
    {
        private readonly CommandsQueriesModules commandsQueriesModules;
        private readonly TypesCatalog catalog;

        public CQRSModule(
            TypesCatalog catalog,
            CommandsQueriesModules commandsQueriesModules)
        {
            this.commandsQueriesModules = commandsQueriesModules;
            this.catalog = catalog;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new ContravariantRegistrationSource());

            builder.RegisterType<AutofacPipelineFactory>().As<IPipelineFactory>()
                .SingleInstance();

            builder.RegisterGeneric(typeof(SecurityElement<,,>)).AsSelf();
            builder.RegisterGeneric(typeof(ValidationElement<>)).AsSelf();
            builder.RegisterGeneric(typeof(CacheElement<>)).AsSelf();
            builder.RegisterGeneric(typeof(CommandFinalizer<>)).AsSelf();
            builder.RegisterGeneric(typeof(QueryFinalizer<>)).AsSelf();
            builder.RegisterGeneric(typeof(EventsInterceptorElement<,,>)).AsSelf();
            builder.RegisterGeneric(typeof(EventsExecutorElement<,,>)).AsSelf();

            builder.RegisterType<AutofacCommandHandlerResolver>().As<ICommandHandlerResolver>();
            builder.RegisterType<AutofacQueryHandlerResolver>().As<IQueryHandlerResolver>();
            builder.RegisterType<AutofacAuthorizerResolver>().As<IAuthorizerResolver>();
            builder.RegisterType<AutofacValidatorResolver>().As<ICommandValidatorResolver>();
            builder.RegisterType<AutofacEventHandlerResolver>().As<IDomainEventHandlerResolver>();

            builder.RegisterType<RoleRegistry>().AsSelf().SingleInstance();
            builder.RegisterType<DefaultPermissionAuthorizer>().AsSelf().AsImplementedInterfaces();

            builder.RegisterType<AsyncEventsInterceptor>()
                .AsSelf()
                .OnActivated(a => a.Instance.Configure())
                .SingleInstance();
            builder.RegisterType<RetryPolicies>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<SimpleEventsExecutor>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<SimpleFinalizer>().AsSelf();

            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(ICommandHandler<,>));
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IQueryHandler<,,>));
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IDomainEventHandler<>));

            foreach (var commandQueryModule in this.commandsQueriesModules)
            {
                builder.RegisterModule(commandQueryModule);
            }
        }
    }
}
