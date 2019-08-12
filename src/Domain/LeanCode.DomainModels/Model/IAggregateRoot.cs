namespace LeanCode.DomainModels.Model
{
    /// <summary> An aggregate root that is also optimistically concurrent </summary>
    public interface IAggregateRoot<TIdentity> : IAggregateRootWithoutOptimisticConcurrency<TIdentity>, IOptimisticConcurrency
    { }

    /// <summary> An aggregate root that is not optimistically concurrent </summary>
    public interface IAggregateRootWithoutOptimisticConcurrency<TIdentity> : IIdentifiable<TIdentity>, IAggregateRootWithoutOptimisticConcurrency
    { }

    /// <summary> Marker interface, do not use explicitly </summary>
    public interface IAggregateRootWithoutOptimisticConcurrency
    { }
}
