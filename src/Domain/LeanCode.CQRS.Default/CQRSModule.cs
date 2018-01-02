using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using AutoMapper;
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

        public CQRSModule WithDefaultPipelines<TAppContext>(TypesCatalog catalog)
            where TAppContext : ISecurityContext, IEventsContext
        {
            return WithCustomPipelines<TAppContext>(
                catalog,
                b => b.Secure().Validate().ExecuteEvents().InterceptEvents(),
                b => b.Secure().Cache()
            );
        }

        public CQRSModule WithCustomPipelines<TAppContext>(
            TypesCatalog catalog,
            CommandBuilder<TAppContext> commandBuilder,
            QueryBuilder<TAppContext> queryBuilder)
            where TAppContext : IPipelineContext
        {
            modules.Add(new TypedCQRSModule<TAppContext>(catalog, commandBuilder, queryBuilder));
            return this;
        }
    }
}
