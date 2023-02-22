using System.Threading;
using System.Threading.Tasks;
using Cronos;

namespace LeanCode.PeriodicService;

public interface IPeriodicAction
{
    CronExpression When { get; }
    bool SkipFirstExecution { get; }
    Task ExecuteAsync(CancellationToken stoppingToken);
}
