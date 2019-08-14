using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public sealed class SimpleEFRepository<TEntity, TIdentity, TContext, TUnitOfWork>
        : EFRepository<TEntity, TIdentity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRoot<TIdentity>
        where TContext : DbContext
        where TUnitOfWork : IUnitOfWork
    {
        public SimpleEFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }

        public override Task<TEntity> FindAsync(TIdentity id)
        {
            return DbSet.FindAsync(id);
        }
    }

    public sealed class SimpleEFRepository<TEntity, TContext, TUnitOfWork>
        : EFRepository<TEntity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRoot<Guid>
        where TContext : DbContext
        where TUnitOfWork : IUnitOfWork
    {
        public SimpleEFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }

        public override Task<TEntity> FindAsync(Guid id)
        {
            return DbSet.FindAsync(id);
        }
    }
}
