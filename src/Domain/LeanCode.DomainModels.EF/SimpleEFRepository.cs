using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    /// <summary>Implementation of <see cref="EFRepository{TEntity, TIdentity, TContext, TUnitOfWork}"/>
    /// for aggregates not having navigation properties, i.e. they do not need any <c>DbSet.Include</c> calls to fetch
    /// the entire aggregate </summary>
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
            return dbSet.FindAsync(id);
        }
    }

    /// <summary>Utility overload of <see cref="SimpleEFRepository{TEntity, TIdentity, TContext, TUnitOfWork}" />
    /// for aggregates with <see cref="System.Guid" /> as Id type </summary>
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
            return dbSet.FindAsync(id);
        }
    }
}
