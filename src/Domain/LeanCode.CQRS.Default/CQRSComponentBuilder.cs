using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using LeanCode.Components;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default
{
    public class CQRSComponentBuilder
    {
        private List<IModule> commandsQueriesModules;
        private TypesCatalog catalog;

        public CQRSComponentBuilder()
        {
            commandsQueriesModules = new List<IModule>();
            catalog = new TypesCatalog(new Assembly[] { });
        }

        public CQRSComponentBuilder WithDefaultPipelines<TAppContext>(TypesCatalog catalog)
            where TAppContext : ISecurityContext, IEventsContext
        {
            return WithCustomPipelines<TAppContext>(
                catalog,
                b => b.Secure().Validate().ExecuteEvents().InterceptEvents(),
                b => b.Secure().Cache()
            );
        }

        public CQRSComponentBuilder WithCustomPipelines<TAppContext>(
            TypesCatalog catalog,
            CommandBuilder<TAppContext> commandBuilder,
            QueryBuilder<TAppContext> queryBuilder)
            where TAppContext : IPipelineContext
        {
            var module = new CommandQueryModule<TAppContext>(
                catalog,
                commandBuilder,
                queryBuilder
            );

            this.commandsQueriesModules.Add(module);
            this.catalog = this.catalog.Merge(catalog);

            return this;
        }

        public CQRSComponent Build()
        {
            return new CQRSComponent(catalog, this.commandsQueriesModules);
        }
    }
}
