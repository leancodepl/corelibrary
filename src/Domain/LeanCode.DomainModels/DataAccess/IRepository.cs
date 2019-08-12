using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.DataAccess
{
    /// <summary>A <see cref="IRepository{TEntity, TIdentity}"/> overload with
    /// <see cref="System.Guid"/> as <c>TIdentity</c> </summary>
    public interface IRepository<TEntity> : IRepository<TEntity, Guid>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<Guid>
    { }

    /// <summary>Interface representing CRUD operations on
    /// <see cref="IAggregateRootWithoutOptimisticConcurrency{TIdentity}" />.
    /// Async versions of Add, Delete, Update methods should save the changes immediatelly (e.g. commit database transaction)
    /// Non-async version of these method should not apply changes. Instead changes should be persisted using <see cref="IUnitOfWork.CommitAsync"/> method.
    /// </summary>
    /// <remarks>In the only current implentation using Entity Framework Core, AddAsync, DeleteAsync, UpdateAsync, DeleteRangeAsync internally commits
    /// all the changes from the underlying DbContext. If you indent to modify multiple objects within single transaction,
    /// use non-async version of the method and commit changes via <see cref="IUnitOfWork.CommitAsync" /> method</remarks>
    public interface IRepository<TEntity, in TIdentity>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
    {
        Task<TEntity> FindAsync(TIdentity id);
        void Add(TEntity entity);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);

        Task AddAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task DeleteRangeAsync(IEnumerable<TEntity> entity);
        Task UpdateAsync(TEntity entity);
    }
}
