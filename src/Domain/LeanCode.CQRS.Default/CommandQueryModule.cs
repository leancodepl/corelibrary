using Autofac;
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
    class CommandQueryModule<TAppContext> : Module
        where TAppContext : IPipelineContext
    {
        private readonly TypesCatalog catalog;
        private readonly CommandBuilder<TAppContext> cmdBuilder;
        private readonly QueryBuilder<TAppContext> queryBuilder;

        public CommandQueryModule(
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
            builder.Register(c => new CommandExecutor<TAppContext>(c.Resolve<IPipelineFactory>(), cmdBuilder))
                .As<ICommandExecutor<TAppContext>>()
                .SingleInstance();
            builder.Register(c => new QueryExecutor<TAppContext>(c.Resolve<IPipelineFactory>(), queryBuilder))
                .As<IQueryExecutor<TAppContext>>()
                .SingleInstance();
        }
    }
}
