namespace LeanCode.DomainModels.Model
{
    public interface IDomainEventHandler<in TEvent>
        where TEvent : IDomainEvent
    {
        void Handle(TEvent domainEvent);
    }
}
