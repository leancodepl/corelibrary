namespace LeanCode.DomainModels.Model
{
    /// <summary>Represents a soft deletable entity</summary>
    public interface ISoftDeletable
    {
        bool IsDeleted { get; }
    }
}
