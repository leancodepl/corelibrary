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
                storageConfig,
                int.MaxValue);
        }

        [Obsolete("Obsolete, use `BackgroundProcessingApp(HangfireConfiguration)` constructor instead.")]
        public BackgroundProcessingApp(string name, string connectionString, string schema)
        {
            configuration = new HangfireConfiguration(name, connectionString, schema);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings());
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(AsyncTaskRunner<,>)).AsSelf();
            builder.RegisterGeneric(typeof(AsyncTaskRunner<>)).AsSelf();

            builder.RegisterType<HangfireScheduler>()
                .WithParameter("queue", configuration.Queue)
                .AsImplementedInterfaces();
            builder.RegisterInstance(configuration);
            builder.RegisterType<AutofacJobActivator>().SingleInstance();
            builder.RegisterType<HangfireInitializer>().SingleInstance().AsImplementedInterfaces();

            var storageOpts = new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = false,
                SchemaName = configuration.Schema,
            };
            configuration.StorageConfig?.Invoke(storageOpts);

            builder.Register(_ => new SqlServerStorage(configuration.ConnectionString, storageOpts))
                .As<JobStorage>()
                .SingleInstance();
        }
    }
}
