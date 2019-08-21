using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using LeanCode.Components;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default
{
    public class CQRSModule : AppModule
    {
        private readonly List<IModule> modules = new List<IModule>(2);

        public CQRSModule()
        {
            modules.Add(new SharedCQRSModule());
        }

        protected override void Load(ContainerBuilder builder)
        {
            foreach (var m in modules)
            {
                builder.RegisterModule(m);
            }
        }

        /// <summary>
        /// Registers CQRS with <typeparamref name="TAppContext" /> application context and default
        /// pipelines:
        ///
        /// Command: Secure->Cache->ExecuteEvents->InterceptEvents
        ///
        /// Query: Secure->Cache
        /// </summary>
        /// <param name="handlersCatalog">Assemblies to register query and command handlers from</param>
        public CQRSModule WithDefaultPipelines<TAppContext>(TypesCatalog handlersCatalog)
            where TAppContext : ISecurityContext, IEventsContext
        {
            return WithCustomPipelines<TAppContext>(
                handlersCatalog,
                b => b.Secure().Validate().ExecuteEvents().InterceptEvents(),
                b => b.Secure().Cache()
            );
        }

        /// <summary>
        /// Registers CQRS with <typeparamref name="TAppContext" /> application context and custom pipelines
        /// </summary>
        /// <param name="handlersCatalog">Assemblies to register query and command handlers from</param>
        /// <param name="commandBuilder">Commands pipeline builder</param>
        /// <param name="queryBuilder">Queries pipeline builder</param>
        public CQRSModule WithCustomPipelines<TAppContext>(
            TypesCatalog handlersCatalog,
            CommandBuilder<TAppContext> commandBuilder,
            QueryBuilder<TAppContext> queryBuilder)
            where TAppContext : IPipelineContext
        {
            modules.Add(new TypedCQRSModule<TAppContext>(handlersCatalog, commandBuilder, queryBuilder));
            return this;
        }
    }
}
