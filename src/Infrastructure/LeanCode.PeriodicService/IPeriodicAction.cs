using System.Threading;
using System.Threading.Tasks;
using Cronos;

namespace LeanCode.PeriodicService;

public interface IPeriodicAction
{
    CronExpression When { get; }
    bool SkipFirstExecution { get; }
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "LNCD0006",
        Justification = "Convention for `PeriodicAction`."
    )]
    Task ExecuteAsync(CancellationToken stoppingToken);
}
