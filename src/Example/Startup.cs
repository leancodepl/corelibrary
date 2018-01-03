using System;
using LeanCode.Cache.AspNet;
using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.RemoteHttp.Server;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.PushNotifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Example
{
    public class Startup : LeanStartup
    {
        private readonly IHostingEnvironment hostEnv;

        private static readonly TypesCatalog searchAssemblies = new TypesCatalog(typeof(Startup));

        public Startup(IConfiguration config, IHostingEnvironment hostEnv)
            : base("LeanCode.Example", config)
        {
            this.hostEnv = hostEnv;
        }

        protected override IAppModule[] Modules { get; } = new IAppModule[]
        {
            new InMemoryCacheModule(),
            new FluentValidationModule(searchAssemblies),
            new CQRSModule().WithDefaultPipelines<AppContext>(searchAssemblies),
            new PushNotificationsModule<Guid>(),

            new ExampleAppModule()
        };

        protected override void ConfigureApp(IApplicationBuilder app)
        {
            if (hostEnv.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var catalog = new TypesCatalog(typeof(Startup));
            app.Map("/api", cfg => cfg.UseRemoteCQRS<AppContext>(catalog, AppContext.FromHttp));

            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
