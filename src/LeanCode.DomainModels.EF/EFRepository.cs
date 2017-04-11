using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public abstract class EFRepository<TEntity, TContext> : EFRepository<TEntity, Guid, TContext>, IRepository<TEntity>
        where TEntity : class, IAggregateRoot<Guid>
        where TContext : DbContext, IUnitOfWork
    {
        public EFRepository(TContext dbContext)
            : base(dbContext)
        { }
    }

    public abstract class EFRepository<TEntity, TIdentity, TContext> : IRepository<TEntity, TIdentity>
        where TEntity: class, IAggregateRoot<TIdentity>
        where TContext : DbContext, IUnitOfWork
    {
        protected readonly DbSet<TEntity> dbSet;

        public EFRepository(TContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            dbSet = dbContext.Set<TEntity>();
        }

        public void Add(TEntity entity) => dbSet.Add(entity);

        public void Delete(TEntity entity) => dbSet.Remove(entity);

        public void DeleteRange(IEnumerable<TEntity> entities) => dbSet.RemoveRange(entities);

        public async Task<TEntity> FindAsync(TIdentity id)
        {
            return ProcessEntity(await LoadAsync(id).ConfigureAwait(false));
        }

        private static TEntity ProcessEntity(TEntity entity)
        {
            if (entity is IOptimisticConcurrency)
            {
                var op = (IOptimisticConcurrency)entity;
                op.DateModified = Time.Now;
            }
            return entity;
        }

        protected abstract Task<TEntity> LoadAsync(TIdentity id);
    }
}
