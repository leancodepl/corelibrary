namespace LeanCode.DomainModels.Model
{
    public interface IDomainEventStorage
    {
        void Store(IDomainEvent domainEvent);
    }
}
