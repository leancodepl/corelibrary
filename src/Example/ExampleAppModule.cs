using System;
using LeanCode.Components;
using LeanCode.CQRS.Security;
using LeanCode.Example.Security;
using LeanCode.Pipelines;
using LeanCode.Pipelines.Autofac;
using LeanCode.PushNotifications;
using LeanCode.PushNotifications.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Example
{
    public class ExampleAppModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDbContext<ExampleDbContext>(opts => opts.UseSqlite("Data Source=example.db"));
            services.AddScoped<IPushNotificationTokenStore<Guid>>(p => new EFPushNotificationTokenStore(p.GetRequiredService<ExampleDbContext>()));
            services.AddMvc();

            services.AddSingleton<IPipelineFactory, AutofacPipelineFactory>();
            services.AddSingleton<IRoleRegistration, AppRoles>();
        }
    }
}
