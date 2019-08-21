using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    /// <summary>Base implementation of <see cref="IRepository{TEntity, TIdentity}"/>.
    /// <c>EFRepository</c> handles modification operations on <typeparamref name="TEntity" />
    /// leaving the details of how to fetch enitties from database to deriving classes
    /// </summary>
    public abstract class EFRepository<TEntity, TIdentity, TContext, TUnitOfWork>
        : IRepository<TEntity, TIdentity>
        where TEntity : class, IAggregateRoot<TIdentity>
        where TContext : DbContext
        where TUnitOfWork : IUnitOfWork
    {
        protected readonly DbSet<TEntity> dbSet;
        protected readonly TUnitOfWork unitOfWork;

        public EFRepository(TContext dbContext, TUnitOfWork unitOfWork)
        {
            dbSet = dbContext.Set<TEntity>();
            this.unitOfWork = unitOfWork;
        }

        public virtual void Add(TEntity entity)
        {
            entity.DateModified = Time.Now;
            dbSet.Add(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            entity.DateModified = Time.Now;
            dbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities)
            {
                e.DateModified = Time.Now;
            }
            dbSet.RemoveRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            entity.DateModified = Time.Now;
            dbSet.Update(entity);
        }

        // NOTE: this may update more than just this one aggregate
        // if there are other objects tracked by EF change tracker
        public virtual Task AddAsync(TEntity entity)
        {
            entity.DateModified = Time.Now;
            dbSet.Add(entity);
            return unitOfWork.CommitAsync();
        }

        public virtual Task DeleteAsync(TEntity entity)
        {
            entity.DateModified = Time.Now;
            dbSet.Remove(entity);
            return unitOfWork.CommitAsync();
        }

        public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities)
            {
                e.DateModified = Time.Now;
            }
            dbSet.RemoveRange(entities);
            return unitOfWork.CommitAsync();
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            entity.DateModified = Time.Now;
            dbSet.Update(entity);
            return unitOfWork.CommitAsync();
        }

        public abstract Task<TEntity> FindAsync(TIdentity id);
    }

    /// <summary> Utility overload of <see cref="EFRepository{TEntity, TIdentity, TContext, TUnitOfWork}" />
    /// for aggregates with <see cref="System.Guid" /> as Id type
    /// </summary>
    public abstract class EFRepository<TEntity, TContext, TUnitOfWork>
        : EFRepository<TEntity, Guid, TContext, TUnitOfWork>, IRepository<TEntity>
        where TEntity : class, IAggregateRoot<Guid>
        where TContext : DbContext
        where TUnitOfWork : IUnitOfWork
    {
        public EFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }
    }
}
