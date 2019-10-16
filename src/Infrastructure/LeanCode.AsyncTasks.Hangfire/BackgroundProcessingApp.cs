using System;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.AsyncTasks.Hangfire
{
    public class BackgroundProcessingApp : AppModule
    {
        private readonly HangfireConfiguration configuration;

        public BackgroundProcessingApp(HangfireConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [Obsolete("Obsolete, use `BackgroundProcessingApp(HangfireConfiguration)` constructor instead.")]
        public BackgroundProcessingApp(
                    string name,
                    string queue,
                    string connectionString,
                    string schema,
                    Action<BackgroundJobServerOptions> serverConfig,
                    Action<SqlServerStorageOptions> storageConfig)
        {
            configuration = new HangfireConfiguration(
                name,
                queue,
                connectionString,
                schema,
                serverConfig,
                storageConfig);
        }

        [Obsolete("Obsolete, use `BackgroundProcessingApp(HangfireConfiguration)` constructor instead.")]
        public BackgroundProcessingApp(string name, string connectionString, string schema)
        {
            configuration = new HangfireConfiguration(name, connectionString, schema);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var storageOpts = new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = true,
                SchemaName = configuration.Schema,
            };
            configuration.StorageConfig?.Invoke(storageOpts);

            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.ConnectionString, storageOpts));

            services.AddHangfireServer(opts =>
            {
                opts.ServerName = configuration.Name;

                if (configuration.Queue is string queue && queue != HangfireConfiguration.DefaultQueue)
                {
                    opts.Queues = new[] { HangfireConfiguration.DefaultQueue, queue };
                }

                configuration.ServerConfig?.Invoke(opts);
            });
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(AsyncTaskRunner<,>)).AsSelf();
            builder.RegisterGeneric(typeof(AsyncTaskRunner<>)).AsSelf();

            builder.RegisterType<HangfireScheduler>()
                .WithParameter("queue", configuration.Queue)
                .AsImplementedInterfaces();
        }
    }
}
