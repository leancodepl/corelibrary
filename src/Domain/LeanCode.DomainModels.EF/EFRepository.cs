using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public abstract class EFRepository<TEntity, TIdentity, TContext, TUnitOfWork> : IRepository<TEntity, TIdentity>
        where TEntity : class, IAggregateRoot<TIdentity>
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
            entity.DateModified = Time.Now;
            DbSet.Add(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            entity.DateModified = Time.Now;
            DbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities)
            {
                e.DateModified = Time.Now;
            }

            DbSet.RemoveRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            entity.DateModified = Time.Now;
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

    public abstract class EFRepository<TEntity, TContext, TUnitOfWork>
        : EFRepository<TEntity, Guid, TContext, TUnitOfWork>, IRepository<TEntity>
        where TEntity : class, IAggregateRoot<Guid>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, IUnitOfWork
    {
        public EFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }
    }
}
