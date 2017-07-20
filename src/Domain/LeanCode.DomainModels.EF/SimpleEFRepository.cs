using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public sealed class SimpleEFRepository<TEntity, TContext> : EFRepository<TEntity, TContext>
        where TEntity : class, IAggregateRoot<Guid>
        where TContext : DbContext, IUnitOfWork
    {
        public SimpleEFRepository(TContext dbContext)
            : base(dbContext)
        { }

        protected override Task<TEntity> LoadAsync(Guid id)
        {
            return dbSet.FindAsync(id);
        }
    }

    public sealed class SimpleEFRepository<TEntity, TIdentity, TContext> : EFRepository<TEntity, TIdentity, TContext>
        where TEntity : class, IAggregateRoot<TIdentity>
        where TContext : DbContext, IUnitOfWork
    {
        public SimpleEFRepository(TContext dbContext)
            : base(dbContext)
        { }

        protected override Task<TEntity> LoadAsync(TIdentity id)
        {
            return dbSet.FindAsync(id);
        }
    }
}
