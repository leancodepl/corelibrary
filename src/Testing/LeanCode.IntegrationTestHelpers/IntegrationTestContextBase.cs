using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LeanCode.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LeanCode.IntegrationTestHelpers
{
    public class IntegrationTestContextBase : IAsyncLifetime
    {
        public IConfiguration Configuration { get; }
        public string DBName { get; }
        public string ConnectionString { get; }

        public bool IsInitialized { get; private set; }
        public IContainer Container { get; private set; }

        public IntegrationTestContextBase()
        {
            IntegrationTestLogging.EnsureLoggerLoaded();

            Configuration = LoadConfiguration();

            DBName = $"integ_{GetType().Name}_{Guid.NewGuid().ToString("N")}";
            ConnectionString = Configuration.GetConnectionString("Database");
            ConnectionString = $"Initial Catalog={DBName};" + ConnectionString;
        }

        public virtual async Task InitializeAsync()
        {
            Container = ConfigureContainerInternal();

            var contexts = Container.Resolve<IEnumerable<DbContext>>();
            foreach (var ctx in contexts)
            {
                await ctx.Database.EnsureCreatedAsync();
            }

            IsInitialized = true;
        }

        public virtual async Task DisposeAsync()
        {
            var contexts = Container.Resolve<IEnumerable<DbContext>>();
            foreach (var ctx in contexts)
            {
                await ctx.Database.EnsureDeletedAsync();
            }

            Container.Dispose();

            IsInitialized = false;
        }

        private IContainer ConfigureContainerInternal()
        {
            var builder = new ContainerBuilder();
            var sc = new ServiceCollection();

            builder.RegisterInstance(CreateLoggerFactory())
                .AsImplementedInterfaces();

            foreach (var component in CreateAppModules())
            {
                builder.RegisterModule(component);
                component.ConfigureServices(sc);
            }

            builder.Populate(sc);

            ConfigureContainer(builder);

            return builder.Build();
        }

        protected virtual IEnumerable<IAppModule> CreateAppModules() => Enumerable.Empty<IAppModule>();
        protected virtual void ConfigureContainer(ContainerBuilder builder) { }

        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }

        private static LoggerFactory CreateLoggerFactory()
        {
            var logginProvider = new Serilog.Extensions.Logging.SerilogLoggerProvider();
            var factory = new LoggerFactory();
            factory.AddProvider(logginProvider);
            return factory;
        }
    }
}
