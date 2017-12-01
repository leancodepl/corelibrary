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
    public class CQRSComponent<TAppContext> : IAppComponent
        where TAppContext : IPipelineContext
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        internal CQRSComponent(
            TypesCatalog catalog,
            CommandBuilder<TAppContext> cmdBuilder,
            QueryBuilder<TAppContext> queryBuilder)
        {
            AutofacModule = new CQRSModule<TAppContext>(catalog, cmdBuilder, queryBuilder);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

    }

    public static class CQRSComponent
    {
        public static CQRSComponent<TAppContext> WithDefaultPipelines<TAppContext>(TypesCatalog catalog)
            where TAppContext : ISecurityContext, IEventsContext
        {
            return WithCustomPipelines<TAppContext>(
                catalog,
                b => b.Secure().Validate().ExecuteEvents().InterceptEvents(),
                b => b.Secure().Cache()
            );
        }

        public static CQRSComponent<TAppContext> WithCustomPipelines<TAppContext>(
            TypesCatalog catalog,
            CommandBuilder<TAppContext> commandBuilder,
            QueryBuilder<TAppContext> queryBuilder)
            where TAppContext : IPipelineContext
        {
            return new CQRSComponent<TAppContext>(catalog, commandBuilder, queryBuilder);
        }
    }
}
