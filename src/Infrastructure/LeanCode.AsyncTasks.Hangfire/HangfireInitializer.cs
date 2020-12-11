using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using LeanCode.OrderedHostedServices;

namespace LeanCode.AsyncTasks.Hangfire
{
    public class HangfireInitializer : IOrderedHostedService
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<HangfireInitializer>();

        private readonly HangfireConfiguration configuration;

        private readonly Func<JobStorage> storageCreator;
        private readonly IEnumerable<IBackgroundProcess> additionalProcesses;
        private readonly JobActivator? jobActivator;
        private readonly IJobFilterProvider? jobFilterProvider;
        private readonly ITimeZoneResolver? timeZoneResolver;

        private BackgroundJobServer? processingServer;

        public int Order => configuration.InitializationOrder;

        public HangfireInitializer(
            HangfireConfiguration configuration,
            Func<JobStorage> storageCreator,
            IEnumerable<IBackgroundProcess> additionalProcesses,
            JobActivator? jobActivator = null,
            IJobFilterProvider? jobFilterProvider = null,
            ITimeZoneResolver? timeZoneResolver = null)
        {
            this.configuration = configuration;
            this.storageCreator = storageCreator;
            this.additionalProcesses = additionalProcesses;
            this.jobActivator = jobActivator;
            this.jobFilterProvider = jobFilterProvider;
            this.timeZoneResolver = timeZoneResolver;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Information("Starting Hangfire");
            var opts = new BackgroundJobServerOptions
            {
                ServerName = configuration.Name,
                Queues = GetQueues(),
            };
            configuration.ServerConfig?.Invoke(opts);

            opts.Activator ??= jobActivator;
            opts.FilterProvider ??= jobFilterProvider;
            opts.TimeZoneResolver ??= timeZoneResolver;

            var storage = storageCreator();
            processingServer = new BackgroundJobServer(opts, storage, additionalProcesses);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Information("Stopping Hangfire");

            if (processingServer != null)
            {
                processingServer.SendStop();
                await processingServer.WaitForShutdownAsync(cancellationToken);
                processingServer.Dispose();
                processingServer = null;
            }
        }

        private string[] GetQueues()
        {
            if (configuration.Queue is string queue && queue != HangfireConfiguration.DefaultQueue)
            {
                return new[] { HangfireConfiguration.DefaultQueue, queue };
            }
            else
            {
                return new[] { HangfireConfiguration.DefaultQueue };
            }
        }
    }
}
