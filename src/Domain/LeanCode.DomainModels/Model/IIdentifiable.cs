namespace LeanCode.DomainModels.Model
{
    /// <summary>Specifes that the object has unique idenity</summary>
    /// <typeparam name="TIdentity">Id type, in most cases <see cref="System.Guid"/> should be used</typeparam>
    public interface IIdentifiable<TIdentity>
    {
        TIdentity Id { get; }
    }
}
