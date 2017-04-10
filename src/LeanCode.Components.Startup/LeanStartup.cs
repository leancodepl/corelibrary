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
    public abstract class LeanStartup<TStartup>
        where TStartup: class
    {
        public const string DatabaseConnectionStringName = "Default";
        public const string SystemLoggersEntryName = "Serilog:SystemLoggers";

        protected readonly IConfigurationRoot Configuration;
        protected readonly IHostingEnvironment HostingEnvironment;
        protected readonly string DefaultConnectionString;

        private readonly Serilog.ILogger logger;

        private IWebApplication[] applications;
        private IAppComponent[] components;

        private IEnumerable<IAppComponent> AllComponents => components.Concat(applications);

        public LeanStartup(string appName, IHostingEnvironment hostEnv)
        {
            HostingEnvironment = hostEnv;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{hostEnv.EnvironmentName}.json", true, true);

            if (hostEnv.IsDevelopment())
            {
                builder = builder.AddUserSecrets<TStartup>();
            }

            builder = builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            DefaultConnectionString = Configuration.GetConnectionString(DatabaseConnectionStringName);

            Log.Logger = new LoggerConfiguration()
                .DestructureCommonObjects(TypesCatalog.Assemblies)
                .EnrichWithAppName(appName)
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            logger = Log.ForContext<LeanStartup<TStartup>>();

            logger.Information("Configuration loaded, starting app");
        }

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            components = CreateComponents();
            applications = CreateApplications();

            logger.Debug("Loading common services");
            services.AddOptions();
            services.AddMvc();

            foreach (var component in AllComponents)
            {
                logger.Debug("Loading services of {Component}", component.GetType());
                component.ConfigureServices(services);
            }

            return ConfigureContainer(services);
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            var opts = Configuration.Options<Dictionary<string, LogLevel>>(SystemLoggersEntryName);
            loggerFactory
                .WithFilter(new FilterLoggerSettings { Switches = opts })
                .AddDebug()
                .AddSerilog();

            foreach (var leanApp in applications
                .OrderByDescending(a => a.BasePath.Length))
            {
                logger.Debug("Mapping app {App} to {BasePath}", leanApp.GetType(), leanApp.BasePath);
                app.MapWhen(
                    c => c.Request.Path.Value.StartsWith(leanApp.BasePath),
                    leanApp.Configure);
            }

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
