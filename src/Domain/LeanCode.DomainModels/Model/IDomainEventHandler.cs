using System.Threading.Tasks;

namespace LeanCode.DomainModels.Model
{
    public interface IDomainEventHandler<in TEvent>
        where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent domainEvent);
    }

    public abstract class SyncDomainEventHandler<TEvent> : IDomainEventHandler<TEvent>
        where TEvent : IDomainEvent
    {
        public Task HandleAsync(TEvent domainEvent)
        {
            Handle(domainEvent);
            return Task.FromResult<object>(null);
        }

        public abstract void Handle(TEvent domainEvent);
    }
}
