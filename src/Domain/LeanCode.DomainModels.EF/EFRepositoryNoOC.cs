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
    where TUnitOfWork : EFUnitOfWorkBase<TContext>
    {
        protected readonly DbSet<TEntity> dbSet;
        protected readonly TUnitOfWork unitOfWork;

        public EFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
        {
            dbSet = dbContext.Set<TEntity>();
            this.unitOfWork = unitOfWork;
        }

        public void Add(TEntity entity) => dbSet.Add(entity);

        public void Delete(TEntity entity) => dbSet.Remove(entity);

        public void DeleteRange(IEnumerable<TEntity> entities) => dbSet.RemoveRange(entities);

        public virtual Task UpdateAsync(TEntity entity)
        {
            dbSet.Update(entity);
            return unitOfWork.CommitAsync();
            // NOTE: this may update more than just this one aggregate
            // if there are other objects tracked by EF change tracker
        }

        public abstract Task<TEntity> FindAsync(TIdentity id);
    }
}
