using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.EventsExecution.Tests
{
    public class PublishingHandler<TEvent> : IDomainEventHandler<TEvent>
        where TEvent : class, IDomainEvent
    {
        private readonly IDomainEvent[] events;
        private int times = 1;

        public PublishingHandler(params IDomainEvent[] events)
        {
            this.events = events;
        }

        public PublishingHandler(int times, params IDomainEvent[] events)
        {
            this.times = times;
            this.events = events;
        }

        public Task HandleAsync(TEvent domainEvent)
        {
            if (times > 0)
            {
                foreach (var e in events)
                {
                    DomainEvents.Raise(e);
                }

                times--;
            }

            return Task.CompletedTask;
        }
    }
}
