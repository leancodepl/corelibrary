using LeanCode.DomainModels.Model;

namespace LeanCode.Example.CQRS
{
    public class SampleEventHandler : SyncDomainEventHandler<SampleEvent>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SampleEventHandler>();

        public override void Handle(SampleEvent domainEvent)
        {
            logger.Information("SampleEventHandler executed for event {EventId}", domainEvent.Id);
        }
    }
}
