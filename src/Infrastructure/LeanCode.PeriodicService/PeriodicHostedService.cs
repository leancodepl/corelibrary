using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using LeanCode.OrderedHostedServices;
using LeanCode.Time;

namespace LeanCode.PeriodicService
{
    public class PeriodicHostedService<TAction> : OrderedBackgroundService
        where TAction : IPeriodicAction
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PeriodicHostedService<TAction>>();

        private readonly ILifetimeScope scope;

        public override int Order { get; }

        public PeriodicHostedService(ILifetimeScope scope, int order)
        {
            this.scope = scope;
            Order = order;
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
            var now = TimeProvider.Now;
            var next = action.When.GetNextOccurrence(now, false) ?? throw new InvalidOperationException("Cannot get next occurrence of the task.");
            return next - now;
        }
    }
}
