using System.Diagnostics.CodeAnalysis;
using Cronos;

namespace LeanCode.PeriodicService;

public interface IPeriodicAction
{
    [SuppressMessage("?", "CA1716", Justification = "Convention for `PeriodicAction`.")]
    CronExpression When { get; }
    bool SkipFirstExecution { get; }

    [SuppressMessage("?", "LNCD0006", Justification = "Convention for `PeriodicAction`.")]
    Task ExecuteAsync(CancellationToken stoppingToken);
}
