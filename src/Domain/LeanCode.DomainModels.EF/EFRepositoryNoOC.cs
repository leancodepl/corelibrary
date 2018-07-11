using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public abstract class EFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
        : IRepository<TEntity, TIdentity>
          where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
          where TContext : DbContext
          where TUnitOfWork : IUnitOfWork
    {
        protected readonly DbSet<TEntity> dbSet;
        protected readonly TUnitOfWork unitOfWork;

        public EFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
        {
            dbSet = dbContext.Set<TEntity>();
            this.unitOfWork = unitOfWork;
        }

        public virtual void Add(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            dbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            dbSet.Update(entity);
        }

        // NOTE: this may update more than just this one aggregate
        // if there are other objects tracked by EF change tracker
        public virtual Task AddAsync(TEntity entity)
        {
            dbSet.Add(entity);
            return unitOfWork.CommitAsync();
        }

        public virtual Task DeleteAsync(TEntity entity)
        {
            dbSet.Remove(entity);
            return unitOfWork.CommitAsync();
        }

        public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            dbSet.RemoveRange(entities);
            return unitOfWork.CommitAsync();
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            dbSet.Update(entity);
            return unitOfWork.CommitAsync();
        }

        public abstract Task<TEntity> FindAsync(TIdentity id);
    }

    public abstract class EFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, Guid, TContext, TUnitOfWork>, IRepository<TEntity>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<Guid>
        where TContext : DbContext
        where TUnitOfWork : IUnitOfWork
    {
        public EFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }
    }
}
