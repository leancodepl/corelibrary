using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.DataAccess
{
    public interface IRepository<TEntity> : IRepository<TEntity, Guid>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<Guid>
    { }

    public interface IRepository<TEntity, in TIdentity>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
    {
        Task<TEntity> FindAsync(TIdentity id);
        void Add(TEntity entity);
        Task UpdateAsync(TEntity entity);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
    }
}
