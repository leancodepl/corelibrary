namespace LeanCode.DomainModels.Model
{
    public interface IAggregateRoot<TIdentity> : IAggregateRootWithoutOptimisticConcurrency<TIdentity>, IOptimisticConcurrency
    { }

    public interface IAggregateRootWithoutOptimisticConcurrency<TIdentity> : IIdentifiable<TIdentity>, IAggregateRootWithoutOptimisticConcurrency
    { }

    public interface IAggregateRootWithoutOptimisticConcurrency
    { }
}
