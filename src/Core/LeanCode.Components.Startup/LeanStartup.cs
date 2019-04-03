using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LeanCode.Components.Startup
{
    public abstract class LeanStartup : IStartup
    {
        protected readonly IConfiguration Configuration;
        protected readonly Serilog.ILogger Logger;

        protected abstract IAppModule[] Modules { get; }

        public LeanStartup(IConfiguration config)
        {
            Configuration = config;

            Logger = Log.ForContext(GetType());

            Logger.Information("Configuration loaded, starting app");
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            Logger.Debug("Loading common services");
            services.AddOptions();

            foreach (var component in Modules)
            {
                Logger.Debug("Loading services of {Component}", component.GetType());
                component.ConfigureServices(services);
            }

            return ConfigureContainer(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            ConfigureApp(app);

            var appLifetime = app.ApplicationServices
                .GetRequiredService<IApplicationLifetime>();
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }

        protected abstract void ConfigureApp(IApplicationBuilder app);

        private IServiceProvider ConfigureContainer(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            builder.Populate(services);
            foreach (var component in Modules)
            {
                builder.RegisterModule(component);
            }

            return builder.Build().Resolve<IServiceProvider>();
        }
    }
}
