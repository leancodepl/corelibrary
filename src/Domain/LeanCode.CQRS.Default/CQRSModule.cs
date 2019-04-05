using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using LeanCode.Components;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.Pipelines;
using Microsoft.Extensions.DependencyInjection;

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

        public CQRSModule WithDefaultPipelines<TAppContext>(TypesCatalog handlersCatalog)
            where TAppContext : ISecurityContext, IEventsContext
        {
            return WithCustomPipelines<TAppContext>(
                handlersCatalog,
                b => b.Secure().Validate().ExecuteEvents().InterceptEvents(),
                b => b.Secure().Cache()
            );
        }

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
