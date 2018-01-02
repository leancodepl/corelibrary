using System.Collections.Generic;
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
    public class CQRSComponent : IAppComponent
    {
        private readonly CombinedCQRSModule module = new CombinedCQRSModule();

        public IModule AutofacModule => module;
        public Profile MapperProfile => null;

        public CQRSComponent()
        {
            module.AddModule(new SharedCQRSModule());
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public CQRSComponent WithDefaultPipelines<TAppContext>(TypesCatalog catalog)
            where TAppContext : ISecurityContext, IEventsContext
        {
            return WithCustomPipelines<TAppContext>(
                catalog,
                b => b.Secure().Validate().ExecuteEvents().InterceptEvents(),
                b => b.Secure().Cache()
            );
        }

        public CQRSComponent WithCustomPipelines<TAppContext>(
            TypesCatalog catalog,
            CommandBuilder<TAppContext> commandBuilder,
            QueryBuilder<TAppContext> queryBuilder)
            where TAppContext : IPipelineContext
        {
            module.AddModule(new CQRSModule<TAppContext>(catalog, commandBuilder, queryBuilder));
            return this;
        }
    }
}
