using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.DomainModels.DataAccess
{
    public interface IUnitOfWork
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
