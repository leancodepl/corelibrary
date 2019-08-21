using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    /// <summary>
    /// Implementation of <see cref="EFRepositoryNoOC{TEntity, TIdentity, TContext, TUnitOfWork}"/>
    /// for non optimistically concurrent aggregates (<see cref="IAggregateRootWithoutOptimisticConcurrency{TIdentity}" />)
    /// without navigation properties, i.e.they do not need any<c>DbSet.Include</c> calls to fetch
    /// the entire aggregate
    /// </summary>
    public class SimpleEFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
          where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
          where TContext : DbContext
          where TUnitOfWork : EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }

        public override Task<TEntity> FindAsync(TIdentity id)
        {
            return dbSet.FindAsync(id);
        }
    }

    /// <summary>Utility overload of <see cref="SimpleEFRepositoryNoOC{TEntity, TIdentity, TContext, TUnitOfWork}" />
    /// for aggregates with <see cref="System.Guid" /> as Id type </summary>
    public class SimpleEFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, TContext, TUnitOfWork>
          where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<Guid>
          where TContext : DbContext
          where TUnitOfWork : EFUnitOfWorkBase<TContext>
    {
        public SimpleEFRepositoryNoOC(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }

        public override Task<TEntity> FindAsync(Guid id)
        {
            return dbSet.FindAsync(id);
        }
    }
}
