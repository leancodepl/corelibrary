using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public abstract class EFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork> : IRepository<TEntity, TIdentity>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
        where TIdentity : notnull
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, IUnitOfWork
    {
        protected DbSet<TEntity> DbSet { get; }
        protected TUnitOfWork UnitOfWork { get; }

        public EFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
        {
            DbSet = dbContext.Set<TEntity>();
            UnitOfWork = unitOfWork;
        }

        public virtual void Add(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            DbSet.RemoveRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        // NOTE: this may update more than just this one aggregate
        // if there are other objects tracked by EF change tracker
        public virtual Task AddAsync(TEntity entity)
        {
            Add(entity);
            return UnitOfWork.CommitAsync();
        }

        public virtual Task DeleteAsync(TEntity entity)
        {
            Delete(entity);
            return UnitOfWork.CommitAsync();
        }

        public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            DeleteRange(entities);
            return UnitOfWork.CommitAsync();
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            Update(entity);
            return UnitOfWork.CommitAsync();
        }

        public abstract ValueTask<TEntity?> FindAsync(TIdentity id);
    }

    public abstract class EFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, Guid, TContext, TUnitOfWork>, IRepository<TEntity>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<Guid>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, IUnitOfWork
    {
        public EFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }
    }
}
