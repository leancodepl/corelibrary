using System;
using Autofac;
using Autofac.Features.Variance;
using LeanCode.Components;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.Pipelines;
using LeanCode.Pipelines.Autofac;

namespace LeanCode.CQRS.Default
{
    using CommandBuilder = Func<PipelineBuilder<ExecutionContext, ICommand, CommandResult>, PipelineBuilder<ExecutionContext, ICommand, CommandResult>>;
    using QueryBuilder = Func<PipelineBuilder<ExecutionContext, IQuery, object>, PipelineBuilder<ExecutionContext, IQuery, object>>;

    class CQRSModule : Module
    {
        private readonly TypesCatalog catalog;
        private readonly CommandBuilder cmdBuilder;
        private readonly QueryBuilder queryBuilder;

        public CQRSModule(
            TypesCatalog catalog,
            CommandBuilder cmdBuilder,
            QueryBuilder queryBuilder)
        {
            this.catalog = catalog;
            this.cmdBuilder = cmdBuilder;
            this.queryBuilder = queryBuilder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new ContravariantRegistrationSource());

            builder.Register(c => new CommandExecutor(c.Resolve<IPipelineFactory>(), cmdBuilder)).As<ICommandExecutor>();
            builder.Register(c => new QueryExecutor(c.Resolve<IPipelineFactory>(), queryBuilder)).As<IQueryExecutor>();

            builder.RegisterType<AutofacPipelineFactory>().As<IPipelineFactory>();
            builder.RegisterType<CommandFinalizer>().AsSelf();
            builder.RegisterType<QueryFinalizer>().AsSelf();

            builder.RegisterGeneric(typeof(SecurityElement<,>)).AsSelf();
            builder.RegisterType<ValidationElement>().AsSelf();
            builder.RegisterType<CacheElement>().AsSelf();

            builder.RegisterType<AutofacCommandHandlerResolver>().As<ICommandHandlerResolver>();
            builder.RegisterType<AutofacQueryHandlerResolver>().As<IQueryHandlerResolver>();
            builder.RegisterType<AutofacAuthorizerResolver>().As<IAuthorizerResolver>();
            builder.RegisterType<AutofacValidatorResolver>().As<ICommandValidatorResolver>();

            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(ICommandHandler<>));
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IQueryHandler<,>));
        }
    }
}
