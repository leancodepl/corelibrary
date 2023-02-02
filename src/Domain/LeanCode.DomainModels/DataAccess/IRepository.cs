using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.DataAccess
{
    public interface IRepository<TEntity, in TIdentity>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
        where TIdentity : notnull
    {
        Task<TEntity?> FindAsync(TIdentity id, CancellationToken cancellationToken = default);

        void Add(TEntity entity);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
    }
}
