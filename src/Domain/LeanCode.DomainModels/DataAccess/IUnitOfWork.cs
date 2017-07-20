using System.Threading.Tasks;

namespace LeanCode.DomainModels.DataAccess
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
    }
}
