using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using LeanCode.TimeProvider;
using Microsoft.Extensions.Hosting;

namespace LeanCode.PeriodicService
{
    public class PeriodicHostedService<TAction> : BackgroundService
        where TAction : IPeriodicAction
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PeriodicHostedService<TAction>>();

        private readonly ILifetimeScope scope;

        public PeriodicHostedService(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var executionNo = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = await ExecuteOnceAsync(executionNo, stoppingToken);
                executionNo++;
                if (!stoppingToken.IsCancellationRequested)
                {
                    logger.Debug("Periodic action executed, waiting {Delay} for the next run", delay);
                    await Task.Delay(delay, stoppingToken);
                }
            }
        }

        private async Task<TimeSpan> ExecuteOnceAsync(int executionNo, CancellationToken stoppingToken)
        {
            using (var innerScope = scope.BeginLifetimeScope())
            {
                var service = innerScope.Resolve<TAction>();
                if (!service.SkipFirstExecution || executionNo > 0)
                {
                    try
                    {
                        await service.ExecuteAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Cannot run periodic action, exception has been thrown");
                    }
                }

                return CalculateDelay(service);
            }
        }

        private static TimeSpan CalculateDelay(IPeriodicAction action)
        {
            var now = Time.Now;
            var next = action.When.GetNextOccurrence(now, false) ?? throw new InvalidOperationException("Cannot get next occurrence of the task.");
            return next - now;
        }
    }
}
