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
        where TIdentity : notnull, IEquatable<TIdentity>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override Task<TEntity?> FindAsync(TIdentity id) =>
            DbSet.AsTracking().FirstOrDefaultAsync(e => e.Id.Equals(id))!; // TODO: remove ! when EF Core adds support for NRT
    }

    public sealed class SimpleEFRepository<TEntity, TContext, TUnitOfWork>
        : EFRepository<TEntity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRoot<Guid>
        where TContext : notnull, DbContext
        where TUnitOfWork : notnull, EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork) { }

        public override Task<TEntity?> FindAsync(Guid id) =>
            DbSet.AsTracking().FirstOrDefaultAsync(e => e.Id == id)!; // TODO: remove ! when EF Core adds support for NRT
    }
}
