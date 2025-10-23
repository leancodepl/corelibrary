using LeanCode.Logging;
using LeanCode.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LeanCode.PeriodicService;

public class PeriodicHostedService<TAction> : BackgroundService
    where TAction : IPeriodicAction
{
    private static readonly TimeSpan MinDelay = TimeSpan.FromMilliseconds(10);

    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<PeriodicHostedService<TAction>> logger;

    public PeriodicHostedService(IServiceProvider serviceProvider, ILogger<PeriodicHostedService<TAction>> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextOccurrence = await ExecuteOnceAsync(executionNo, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                logger.Debug(
                    "Periodic action executed, the next run will be at {NextOccurrence}",
                    nextOccurrence
                );
                do
                {
                    var now = TimeProvider.Time.UtcNow;
                    var delay = nextOccurrence - now < MinDelay ? MinDelay : nextOccurrence - now;

                    await Task.Delay(delay, stoppingToken);
                }
                while (!stoppingToken.IsCancellationRequested && TimeProvider.Time.UtcNow <= nextOccurrence);
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031",
        Justification = "The method is an exception boundary."
    )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "LNCD0006",
        Justification = "Convention for `PeriodicAction`."
    )]
    private async Task<DateTimeOffset> ExecuteOnceAsync(int executionNo, CancellationToken stoppingToken)
    {
        using var activity = LeanCodeActivitySource.StartExecution("Periodic action", typeof(TAction).Name);
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
                activity?.AddException(ex);
                logger.Error(ex, "Cannot run periodic action, exception has been thrown");
            }
        }

        return CalculateNextOccurrence(service);
    }

    private static DateTimeOffset CalculateNextOccurrence(IPeriodicAction action)
    {
        var now = TimeProvider.Time.UtcNow;
        var next =
            action.When.GetNextOccurrence(now)
            ?? throw new InvalidOperationException("Cannot get next occurrence of the task.");
        return next;
    }
}
