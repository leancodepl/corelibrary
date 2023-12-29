namespace LeanCode.DomainModels.Model;

public interface IAggregateRoot<TIdentity>
    : IAggregateRootWithoutOptimisticConcurrency<TIdentity>,
        IOptimisticConcurrency
    where TIdentity : notnull { }

public interface IAggregateRootWithoutOptimisticConcurrency<TIdentity>
    : IEntity<TIdentity>,
        IAggregateRootWithoutOptimisticConcurrency
    where TIdentity : notnull { }

[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1040", Justification = "Marker interface.")]
public interface IAggregateRootWithoutOptimisticConcurrency { }

[Obsolete("`IIdentifiable` is obsolete. Use `IEntity` instead.")]
public interface IIdentifiable<TIdentity> : IEntity<TIdentity>
    where TIdentity : notnull { }
