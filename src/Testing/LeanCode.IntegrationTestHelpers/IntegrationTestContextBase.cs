using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using LeanCode.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LeanCode.IntegrationTestHelpers
{

    public abstract class IntegrationTestContextBase : IAsyncLifetime
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
            ConnectionString = Configuration.GetConnectionString("Default");
            ConnectionString += $"Initial Catalog={DBName};";
        }

        public virtual async Task InitializeAsync()
        {
            Container = ConfigureContainerInternal();

            var dbContext = Container.Resolve<DbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            IsInitialized = true;
        }

        public virtual async Task DisposeAsync()
        {
            var dbContext = Container.Resolve<DbContext>();
            await dbContext.Database.EnsureDeletedAsync();

            Container.Dispose();

            IsInitialized = false;
        }

        private IContainer ConfigureContainerInternal()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(CreateLoggerFactory()).AsImplementedInterfaces();

            foreach (var component in CreateAppModules())
            {
                builder.RegisterModule(component);
            }

            ConfigureContainer(builder);


            return builder.Build();
        }

        private static LoggerFactory CreateLoggerFactory()
        {
            var factory = new LoggerFactory();
            var logginProvider = new Serilog.Extensions.Logging.SerilogLoggerProvider();
            factory.AddProvider(logginProvider);
            return factory;
        }

        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        protected DbContextOptionsBuilder<TContext> PrepareDbOptions<TContext>(
            IComponentContext context)
            where TContext : DbContext
        {
            var factory = context.Resolve<ILoggerFactory>();
            return new DbContextOptionsBuilder<TContext>()
                .UseLoggerFactory(factory)
                .UseSqlServer(ConnectionString);
        }

        protected abstract IEnumerable<IAppModule> CreateAppModules();
        protected virtual void ConfigureContainer(ContainerBuilder builder)
        { }
    }
}
