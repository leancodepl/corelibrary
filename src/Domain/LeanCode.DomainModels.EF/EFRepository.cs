using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public abstract class EFRepository<TEntity, TIdentity, TContext, TUnitOfWork> : IRepository<TEntity, TIdentity>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
        where TIdentity : notnull
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, IUnitOfWork
    {
        protected DbSet<TEntity> DbSet { get; }
        protected TUnitOfWork UnitOfWork { get; }

        public EFRepository(TContext dbContext, TUnitOfWork unitOfWork)
        {
            DbSet = dbContext.Set<TEntity>();
            UnitOfWork = unitOfWork;
        }

        public virtual void Add(TEntity entity)
        {
            if (entity is IOptimisticConcurrency oc)
            {
                oc.DateModified = Time.Now;
            }

            DbSet.Add(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            if (entity is IOptimisticConcurrency oc)
            {
                oc.DateModified = Time.Now;
            }

            DbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            foreach (var oc in entities.OfType<IOptimisticConcurrency>())
            {
                oc.DateModified = Time.Now;
            }

            DbSet.RemoveRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            if (entity is IOptimisticConcurrency oc)
            {
                oc.DateModified = Time.Now;
            }

            DbSet.Update(entity);
        }

        // NOTE: this may update more than just this one aggregate
        // if there are other objects tracked by EF change tracker
        public virtual Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Add(entity);
            return UnitOfWork.CommitAsync(cancellationToken);
        }

        public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Delete(entity);
            return UnitOfWork.CommitAsync(cancellationToken);
        }

        public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            DeleteRange(entities);
            return UnitOfWork.CommitAsync(cancellationToken);
        }

        public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Update(entity);
            return UnitOfWork.CommitAsync(cancellationToken);
        }

        public abstract Task<TEntity?> FindAsync(TIdentity id, CancellationToken cancellationToken = default);
    }

    public abstract class EFRepository<TEntity, TContext, TUnitOfWork>
        : EFRepository<TEntity, Id<TEntity>, TContext, TUnitOfWork>, IRepository<TEntity>
        where TEntity : class, IAggregateRoot<Id<TEntity>>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, IUnitOfWork
    {
        public EFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }
    }
}
