namespace LeanCode.DomainModels.Model;

public interface IEntity<TIdentity>
    where TIdentity : notnull
{
    TIdentity Id { get; }
}
