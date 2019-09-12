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
        where TIdentity : notnull
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, IUnitOfWork
    {
        public SimpleEFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override ValueTask<TEntity?> FindAsync(TIdentity id) =>
            DbSet.FindAsync(id)!; // TODO: remove ! when EF Core adds support for NRT
    }

    public sealed class SimpleEFRepository<TEntity, TContext, TUnitOfWork>
        : EFRepository<TEntity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRoot<Guid>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, IUnitOfWork
    {
        public SimpleEFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override ValueTask<TEntity?> FindAsync(Guid id) =>
            DbSet.FindAsync(id)!; // TODO: remove ! when EF Core adds support for NRT
    }
}
