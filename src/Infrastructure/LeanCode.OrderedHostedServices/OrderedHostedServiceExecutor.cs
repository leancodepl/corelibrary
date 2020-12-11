using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace LeanCode.OrderedHostedServices
{
    public sealed class OrderedHostedServiceExecutor : IHostedService
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<OrderedHostedServiceExecutor>();

        private readonly IEnumerable<IOrderedHostedService> services;

        public OrderedHostedServiceExecutor(IEnumerable<IOrderedHostedService> services)
        {
            this.services = services.OrderBy(i => i.Order).ToList();
        }

        public Task StartAsync(CancellationToken token)
        {
            logger.Information("Starting ordered hosted services");
            return ExecuteAsync(
                "Starting {Type}",
                services,
                s => s.StartAsync(token));
        }

        public Task StopAsync(CancellationToken token)
        {
            logger.Information("Stopping ordered hosted services");

            return ExecuteAsync(
                "Stopping {Type}",
                services.Reverse(),
                s => s.StopAsync(token),
                throwOnFirstFailure: false);
        }

        private async Task ExecuteAsync(
            string message,
            IEnumerable<IOrderedHostedService> ordered,
            Func<IHostedService, Task> callback,
            bool throwOnFirstFailure = true)
        {
            List<Exception>? exceptions = null;

            foreach (var service in ordered)
            {
                try
                {
                    await callback(service);
                    logger.Debug(message, service.GetType());
                }
                catch (Exception ex)
                {
                    if (throwOnFirstFailure)
                    {
                        throw;
                    }

                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }

            // Throw an aggregate exception if there were any exceptions
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
