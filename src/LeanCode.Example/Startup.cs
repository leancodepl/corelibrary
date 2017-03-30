using System;
using LeanCode.Cache.AspNet;
using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.CQRS.Autofac;
using LeanCode.CQRS.FluentValidation;
using LeanCode.CQRS.MvcValidation;
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

        protected override Type[] SearchAssemblies
        {
            get
            {
                return new[]
                {
                    typeof(Startup)
                };
            }
        }

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
                new CQRSComponent(SearchAssemblies),
                new FluentValidationComponent(SearchAssemblies),
                new MvcValidationComponent(SearchAssemblies),

                new MvcComponent()
            };
        }
    }
}
