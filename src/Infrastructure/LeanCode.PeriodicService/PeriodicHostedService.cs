using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LeanCode.PeriodicService;

public class PeriodicHostedService<TAction> : BackgroundService
    where TAction : IPeriodicAction
{
    private readonly IServiceProvider serviceProvider;
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PeriodicHostedService<TAction>>();

    public PeriodicHostedService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031",
        Justification = "The method is an exception boundary."
    )]
    private async Task<TimeSpan> ExecuteOnceAsync(int executionNo, CancellationToken stoppingToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<TAction>();
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

    private static TimeSpan CalculateDelay(IPeriodicAction action)
    {
        var now = TimeProvider.Time.Now;
        var next =
            action.When.GetNextOccurrence(now, false)
            ?? throw new InvalidOperationException("Cannot get next occurrence of the task.");
        return next - now;
    }
}
