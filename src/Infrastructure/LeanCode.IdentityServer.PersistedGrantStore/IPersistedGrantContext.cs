using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    public interface IPersistedGrantContext
    {
        DbSet<PersistedGrantEntity> PersistedGrants { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
