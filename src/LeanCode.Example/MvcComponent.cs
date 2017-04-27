using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LeanCode.PushNotifications;
using System;
using LeanCode.PushNotifications.EF;

namespace LeanCode.Example
{
    public class MvcComponent : IAppComponent
    {
        public IModule AutofacModule => null;
        public Profile MapperProfile => null;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ExampleDbContext>(opts => opts.UseSqlite("Data Source=example.db"));
            services.AddScoped<IPushNotificationTokenStore<Guid>>(p => new EFPushNotificationTokenStore(p.GetRequiredService<ExampleDbContext>()));
            services.AddMvc();
        }
    }
}
