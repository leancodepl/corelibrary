using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    public interface IPersistedGrantContext
    {
        DbSet<PersistedGrantEntity> PersistedGrants { get; }
        Task SaveChangesAsync();
    }
}
