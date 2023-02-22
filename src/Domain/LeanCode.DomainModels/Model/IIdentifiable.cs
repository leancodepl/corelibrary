namespace LeanCode.DomainModels.Model;

public interface IIdentifiable<TIdentity>
    where TIdentity : notnull
{
    TIdentity Id { get; }
}
