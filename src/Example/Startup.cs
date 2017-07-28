using System;
using LeanCode.Cache.AspNet;
using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.RemoteHttp.Server;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.PushNotifications;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
                new FluentValidationComponent(TypesCatalog),
                CQRSComponent.WithDefaultPipelines<AppContext>(TypesCatalog),
                RemoteCQRSHttpComponent.Create(TypesCatalog, ctx => new AppContext { User = ctx.User }),
                PushNotificationsComponent<Guid>.WithConfiguration(Configuration),

                new MvcComponent()
            };
        }
    }
}
