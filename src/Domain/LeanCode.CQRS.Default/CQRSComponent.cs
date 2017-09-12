using System.Collections.Generic;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using LeanCode.CQRS.Security;
using LeanCode.DomainModels.EventsExecution;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Default
{
    public class CQRSComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        internal CQRSComponent(
            TypesCatalog catalog,
            List<IModule> commandsQueriesModules)
        {
            AutofacModule = new CQRSModule(catalog, commandsQueriesModules);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public static CQRSComponentBuilder New()
        {
            return new CQRSComponentBuilder();
        }

        public static CQRSComponent WithDefaultPipelines<TAppContext>(TypesCatalog catalog)
            where TAppContext : ISecurityContext, IEventsContext
        {
            return New().WithDefaultPipelines<TAppContext>(catalog).Build();
        }
    }
}
