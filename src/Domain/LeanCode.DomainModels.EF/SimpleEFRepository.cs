using System;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public sealed class SimpleEFRepository<TEntity, TIdentity, TContext, TUnitOfWork>
        : EFRepository<TEntity, TIdentity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRoot<TIdentity>
        where TIdentity : notnull, IEquatable<TIdentity>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, IUnitOfWork
    {
        public SimpleEFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override Task<TEntity?> FindAsync(TIdentity id, CancellationToken cancellationToken = default) =>
            DbSet.AsTracking().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken)!; // TODO: remove ! when EF Core adds support for NRT
    }

    public sealed class SimpleEFRepository<TEntity, TContext, TUnitOfWork>
        : EFRepository<TEntity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRoot<Id<TEntity>>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, IUnitOfWork
    {
        public SimpleEFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override Task<TEntity?> FindAsync(Id<TEntity> id, CancellationToken cancellationToken = default) =>
            DbSet.AsTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)!; // TODO: remove ! when EF Core adds support for NRT
    }
}
