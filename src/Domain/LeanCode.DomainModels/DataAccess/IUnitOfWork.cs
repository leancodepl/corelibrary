using System.Threading.Tasks;

namespace LeanCode.DomainModels.DataAccess
{
    /// <summary> Provides means of commiting database transaction </summary>
    public interface IUnitOfWork
    {
        Task CommitAsync();
    }
}
