namespace LeanCode.DomainModels.Model
{
    public interface IIdentifiable<TIdentity>
    {
        TIdentity Id { get; }
    }
}
