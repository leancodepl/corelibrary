using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
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

        public LeanStartup(string appName, IConfiguration config)
            : this(appName, config, (TypesCatalog)null)
        { }

        public LeanStartup(string appName, IConfiguration config, params Assembly[] assemblies)
            : this(appName, config, new TypesCatalog(assemblies))
        { }

        public LeanStartup(string appName, IConfiguration config, params Type[] types)
            : this(appName, config, new TypesCatalog(types))
        { }

        public LeanStartup(string appName, IConfiguration config, TypesCatalog destructurers)
        {
            Configuration = config;

            var logCfg = new LoggerConfiguration()
                .EnrichWithAppName(appName)
                .ReadFrom.Configuration(Configuration);
            if (destructurers != null)
            {
                logCfg = logCfg.DestructureCommonObjects(destructurers.Assemblies);
            }
            Log.Logger = logCfg.CreateLogger();
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

            ConfigureMapper(builder);

            return builder.Build().Resolve<IServiceProvider>();
        }

        private void ConfigureMapper(ContainerBuilder builder)
        {
            var mapperCfg = new MapperConfiguration(cfg =>
                {
                    foreach (var component in Modules)
                    {
                        if (component.MapperProfile != null)
                        {
                            cfg.AddProfile(component.MapperProfile);
                        }
                    }
                });
            mapperCfg.AssertConfigurationIsValid();

            builder.RegisterInstance(mapperCfg)
                .AsSelf()
                .As<AutoMapper.IConfigurationProvider>();

            builder.Register(ctx =>
                ctx.Resolve<MapperConfiguration>().CreateMapper(ctx.Resolve))
                .As<IMapper>()
                .InstancePerLifetimeScope();
        }
    }
}
