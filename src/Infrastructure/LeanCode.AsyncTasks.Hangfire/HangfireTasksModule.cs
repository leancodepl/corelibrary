using System;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using LeanCode.Components;
using LeanCode.OrderedHostedServices;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.AsyncTasks.Hangfire
{
    public class HangfireTasksModule : AppModule
    {
        private readonly HangfireConfiguration configuration;

        public HangfireTasksModule(HangfireConfiguration configuration)
        {
            this.configuration = configuration;
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
            builder.RegisterOrderedHostedService<HangfireInitializer>().SingleInstance();

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
