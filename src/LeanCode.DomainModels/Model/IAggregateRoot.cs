namespace LeanCode.DomainModels.Model
{
    public interface IAggregateRoot<TIdentity> : IIdentifiable<TIdentity>, IAggregateRoot
    { }

    public interface IAggregateRoot
    { }
}
