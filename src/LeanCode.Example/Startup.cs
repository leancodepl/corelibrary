using System;
using LeanCode.Cache.AspNet;
using LeanCode.DomainModels.Autofac;
using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.CQRS.Autofac;
using LeanCode.CQRS.FluentValidation;
using LeanCode.CQRS.MvcValidation;
using LeanCode.CQRS.RemoteHttp.Server;
using LeanCode.DomainModels.RequestEventsExecutor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LeanCode.PushNotifications;

namespace LeanCode.Example
{
    public class Startup : LeanStartup<Startup>
    {
        public Startup(IHostingEnvironment hostEnv)
            : base("LeanCode.Example", hostEnv)
        { }

        // This is required for EF migrations to inject services properly
        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return base.ConfigureServices(services);
        }

        protected override TypesCatalog TypesCatalog { get; } = new TypesCatalog(
            typeof(Startup)
        );

        protected override IWebApplication[] CreateApplications()
        {
            return new IWebApplication[]
            {
                new WebApp(Configuration, HostingEnvironment)
            };
        }

        protected override IAppComponent[] CreateComponents()
        {
            return new IAppComponent[]
            {
                new InMemoryCacheComponent(),
                new CQRSComponent(TypesCatalog),
                new FluentValidationComponent(TypesCatalog),
                new MvcValidationComponent(TypesCatalog),
                new DomainModelsComponent(TypesCatalog),
                new RequestEventsExecutorComponent(),
                new RemoteCQRSHttpComponent(TypesCatalog),
                new PushNotificationsComponent(),

                new MvcComponent()
            };
        }
    }
}
