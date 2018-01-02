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

    public class CQRSModule<TAppContext> : Module
        where TAppContext : IPipelineContext
    {
        private readonly TypesCatalog catalog;
        private readonly CommandBuilder<TAppContext> cmdBuilder;
        private readonly QueryBuilder<TAppContext> queryBuilder;

        public CQRSModule(
            TypesCatalog catalog,
            CommandBuilder<TAppContext> cmdBuilder,
            QueryBuilder<TAppContext> queryBuilder)
        {
            this.catalog = catalog;
            this.cmdBuilder = cmdBuilder;
            this.queryBuilder = queryBuilder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new ContravariantRegistrationSource());

            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(ICommandHandler<,>));
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IQueryHandler<,,>));
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IDomainEventHandler<>));
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IObjectContextFromAppContextFactory<,>));

            builder.RegisterType<AutofacCommandHandlerResolver<TAppContext>>().As<ICommandHandlerResolver<TAppContext>>();
            builder.RegisterType<AutofacQueryHandlerResolver<TAppContext>>().As<IQueryHandlerResolver<TAppContext>>();
            builder.RegisterType<AutofacAuthorizerResolver<TAppContext>>().As<IAuthorizerResolver<TAppContext>>();
            builder.RegisterType<AutofacValidatorResolver<TAppContext>>().As<ICommandValidatorResolver<TAppContext>>();

            builder.Register(c =>
                new CommandExecutor<TAppContext>(
                    c.Resolve<IPipelineFactory>(),
                    cmdBuilder,
                    c.Resolve<ILifetimeScope>()))
                .As<ICommandExecutor<TAppContext>>()
                .SingleInstance();
            builder.Register(c =>
                new QueryExecutor<TAppContext>(
                    c.Resolve<IPipelineFactory>(),
                    queryBuilder,
                    c.Resolve<ILifetimeScope>()))
                .As<IQueryExecutor<TAppContext>>()
                .SingleInstance();
        }
    }
}
