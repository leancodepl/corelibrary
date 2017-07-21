using System.Threading.Tasks;
using LeanCode.DomainModels.EventsExecution;

namespace LeanCode.Example.CQRS
{
    public class SampleEventHandler : IDomainEventHandler<SampleEvent>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SampleEventHandler>();

        public Task HandleAsync(SampleEvent domainEvent)
        {
            logger.Information("SampleEventHandler executed for event {EventId}", domainEvent.Id);
            return Task.CompletedTask;
        }
    }
}
