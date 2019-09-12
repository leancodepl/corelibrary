using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LeanCode.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Xunit;

namespace LeanCode.IntegrationTestHelpers
{
    public class IntegrationTestContextBase : IAsyncLifetime
    {
        public IConfiguration Configuration { get; }
        public string DBName { get; }
        public string ConnectionString { get; }

        public bool IsInitialized { get; private set; }

        private IContainer? container;

        [AllowNull]
        public IContainer Container
        {
            get => container ?? throw new NullReferenceException();
            private set => container = value;
        }

        public IntegrationTestContextBase()
        {
            RuntimeHelpers.RunClassConstructor(typeof(IntegrationTestLogging).TypeHandle);

            Configuration = LoadConfiguration();

            DBName = $"integ_{GetType().Name}_{Guid.NewGuid().ToString("N")}";
            ConnectionString = Configuration.GetConnectionString("Database");
            ConnectionString = $"Initial Catalog={DBName};" + ConnectionString;
        }

        public ILifetimeScope BeginLifetimeScope() => Container.BeginLifetimeScope();

        public T Resolve<T>() => Container.Resolve<T>();

        public async Task Scoped(Func<ILifetimeScope, Task> action)
        {
            using var sc = BeginLifetimeScope();

            await action(sc);
        }

        public async Task<TResult> Scoped<TResult>(Func<ILifetimeScope, Task<TResult>> action)
        {
            using var sc = BeginLifetimeScope();

            return await action(sc);
        }

        public void Scoped(Action<ILifetimeScope> action)
        {
            using var sc = BeginLifetimeScope();

            action(sc);
        }

        public TResult Scoped<TResult>(Func<ILifetimeScope, TResult> action)
        {
            using var sc = BeginLifetimeScope();

            return action(sc);
        }

        public async Task With<T>(Func<T, Task> action)
        {
            using var sc = BeginLifetimeScope();

            var resource = sc.Resolve<T>();

            if (resource is IAsyncDisposable ad)
            {
                await using (ad)
                {
                    await action(resource);
                }
            }
            else if (resource is IDisposable d)
            {
                using (d)
                {
                    await action(resource);
                }
            }
            else
            {
                await action(resource);
            }
        }

        public async Task<TResult> With<T, TResult>(Func<T, Task<TResult>> action)
        {
            using var sc = BeginLifetimeScope();

            var resource = sc.Resolve<T>();

            if (resource is IAsyncDisposable ad)
            {
                await using (ad)
                {
                    return await action(resource);
                }
            }
            else if (resource is IDisposable d)
            {
                using (d)
                {
                    return await action(resource);
                }
            }
            else
            {
                return await action(resource);
            }
        }

        public void With<T>(Action<T> action)
        {
            using var sc = BeginLifetimeScope();

            var resource = sc.Resolve<T>();

            if (resource is IDisposable d)
            {
                using (d)
                {
                    action(resource);
                }
            }
            else
            {
                action(resource);
            }
        }

        public TResult With<T, TResult>(Func<T, TResult> action)
        {
            using var sc = BeginLifetimeScope();

            var resource = sc.Resolve<T>();

            if (resource is IDisposable d)
            {
                using (d)
                {
                    return action(resource);
                }
            }
            else
            {
                return action(resource);
            }
        }

        public virtual async Task InitializeAsync()
        {
            Container = ConfigureContainerInternal();

            foreach (var ctx in Container.Resolve<IEnumerable<DbContext>>())
            {
                await ctx.Database.EnsureCreatedAsync();
            }

            IsInitialized = true;
        }

        public virtual async Task DisposeAsync()
        {
            foreach (var ctx in Container.Resolve<IEnumerable<DbContext>>())
            {
                await ctx.Database.EnsureDeletedAsync();
            }

            Container.Dispose();
            Container = null;

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
            var factory = new LoggerFactory();

            factory.AddProvider(new SerilogLoggerProvider());

            return factory;
        }
    }
}
