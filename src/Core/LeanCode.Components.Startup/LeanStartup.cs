using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using LeanCode.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LeanCode.Components.Startup
{
    public abstract class LeanStartup : IStartup
    {
        public const string SystemLoggersEntryName = "Serilog:SystemLoggers";

        protected readonly IConfiguration Configuration;

        private readonly Serilog.ILogger logger;

        private IWebApplication[] applications;
        private IAppComponent[] components;

        private IEnumerable<IAppComponent> AllComponents => components.Concat(applications);

        public LeanStartup(string appName, IConfiguration config)
        {
            Configuration = config;

            Log.Logger = new LoggerConfiguration()
                .DestructureCommonObjects(TypesCatalog.Assemblies)
                .EnrichWithAppName(appName)
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            logger = Log.ForContext<LeanStartup>();

            logger.Information("Configuration loaded, starting app");
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            components = CreateComponents();
            applications = CreateApplications();

            logger.Debug("Loading common services");
            services.AddOptions();

            foreach (var component in AllComponents)
            {
                logger.Debug("Loading services of {Component}", component.GetType());
                component.ConfigureServices(services);
            }

            return ConfigureContainer(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            foreach (var leanApp in applications.OrderByDescending(a => a.BasePath.Length))
            {
                logger.Debug("Mapping app {App} to {BasePath}", leanApp.GetType(), leanApp.BasePath);
                if (string.IsNullOrEmpty(leanApp.BasePath) || leanApp.BasePath == "/")
                {
                    leanApp.Configure(app);
                }
                else
                {
                    app.Map(leanApp.BasePath, leanApp.Configure);
                }
            }

            var appLifetime = app.ApplicationServices
                .GetRequiredService<IApplicationLifetime>();
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }

        private IServiceProvider ConfigureContainer(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            builder.Populate(services);

            foreach (var component in AllComponents)
            {
                if (component.AutofacModule != null)
                {
                    builder.RegisterModule(component.AutofacModule);
                }
            }

            ConfigureMapper(builder);

            return builder.Build().Resolve<IServiceProvider>();
        }

        private void ConfigureMapper(ContainerBuilder builder)
        {
            var mapperCfg = new MapperConfiguration(cfg =>
                {
                    foreach (var component in AllComponents)
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

        protected abstract IAppComponent[] CreateComponents();
        protected abstract IWebApplication[] CreateApplications();
        protected abstract TypesCatalog TypesCatalog { get; }
    }
}
