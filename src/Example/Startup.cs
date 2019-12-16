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
using Microsoft.Extensions.Hosting;

namespace LeanCode.Example
{
    public class Startup : LeanStartup
    {
        private static readonly TypesCatalog SearchAssemblies = new TypesCatalog(typeof(Startup));

        private readonly IWebHostEnvironment hostEnv;

        public Startup(IConfiguration config, IWebHostEnvironment hostEnv)
            : base(config)
        {
            this.hostEnv = hostEnv;
        }

        protected override IAppModule[] Modules { get; } = new IAppModule[]
        {
            new InMemoryCacheModule(),
            new FluentValidationModule(SearchAssemblies),
            new CQRSModule().WithDefaultPipelines<AppContext>(SearchAssemblies),
#pragma warning disable CS0618
            new PushNotificationsModule<Guid>(),
#pragma warning restore CS0618

            new ExampleAppModule(),
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
