using System.Threading.Tasks;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    public class FirstEvent1Consumer : IConsumer<Event1>
    {
        private readonly HandledEventsReporter<Event1> reporter;

        public FirstEvent1Consumer(HandledEventsReporter<Event1> reporter)
        {
            this.reporter = reporter;
        }

        public Task Consume(ConsumeContext<Event1> context)
        {
            reporter.ReportEvent(this, context);
            return Task.CompletedTask;
        }
    }
}
