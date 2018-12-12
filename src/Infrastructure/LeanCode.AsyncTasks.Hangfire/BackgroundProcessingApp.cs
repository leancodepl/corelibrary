using System;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using LeanCode.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.AsyncTasks.Hangfire
{
    public class BackgroundProcessingApp : AppModule
    {
        public const string DefaultQueue = "default";

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<BackgroundProcessingApp>();

        private readonly string name;
        private readonly string queue;

        private readonly string connectionString;
        private readonly SqlServerStorageOptions storageOpts;
        private BackgroundJobServer backgroundServer;

        private readonly Action<BackgroundJobServerOptions> serverConfig;

        public BackgroundProcessingApp(
            string name,
            string queue,
            string connectionString,
            string schema,
            Action<BackgroundJobServerOptions> serverConfig,
            Action<SqlServerStorageOptions> storageConfig)
        {
            this.name = name;
            this.queue = queue;
            this.connectionString = connectionString;
            this.storageOpts = new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = true,
                SchemaName = schema
            };
            storageConfig(this.storageOpts);

            this.serverConfig = serverConfig;
        }

        public BackgroundProcessingApp(string name, string connectionString, string schema)
            : this(name, DefaultQueue, connectionString, schema, _ => { }, _ => { })
        { }

        public void ConfigureApp(IApplicationBuilder app)
        {
            var scope = app.ApplicationServices.GetRequiredService<ILifetimeScope>();
            var appLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString, storageOpts);
            GlobalConfiguration.Configuration.UseAutofacActivator(scope);

            appLifetime.ApplicationStarted.Register(StartProcessing);
            appLifetime.ApplicationStopping.Register(StopProcessing);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(cfg =>
               cfg.UseSqlServerStorage(connectionString, storageOpts));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(AsyncTaskRunner<,>))
                .AsSelf();
            builder.RegisterGeneric(typeof(AsyncTaskRunner<>))
                .AsSelf();

            builder.RegisterType<HangfireScheduler>()
                .WithParameter("queue", queue)
                .AsImplementedInterfaces();
        }

        private void StartProcessing()
        {
            logger.Information("Starting background server");
            var cfg = new BackgroundJobServerOptions
            {
                ServerName = name
            };
            if (!(queue is DefaultQueue))
            {
                cfg.Queues = new[] { DefaultQueue, queue };
            }
            serverConfig(cfg);
            backgroundServer = new BackgroundJobServer(cfg);
        }

        private void StopProcessing()
        {
            logger.Information("App is stopping, disposing background server");
            backgroundServer.Dispose();
        }
    }
}
