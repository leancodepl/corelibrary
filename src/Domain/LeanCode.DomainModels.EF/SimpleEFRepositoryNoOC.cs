using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public class SimpleEFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
        where TIdentity : notnull
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override ValueTask<TEntity?> FindAsync(TIdentity id) =>
            DbSet.FindAsync(id)!; // TODO: remove ! when EF Core adds support for NRT
    }

    public class SimpleEFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<Guid>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override ValueTask<TEntity?> FindAsync(Guid id) =>
            DbSet.FindAsync(id)!; // TODO: remove ! when EF Core adds support for NRT
    }
}
