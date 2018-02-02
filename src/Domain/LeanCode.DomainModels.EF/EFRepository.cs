using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public abstract class EFRepository<TEntity, TContext, TUnitOfWork>
        : EFRepository<TEntity, Guid, TContext, TUnitOfWork>, IRepository<TEntity>
        where TEntity : class, IAggregateRoot<Guid>
        where TContext : DbContext
        where TUnitOfWork : EFUnitOfWorkBase<TContext>
    {
        public EFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }
    }

    public abstract class EFRepository<TEntity, TIdentity, TContext, TUnitOfWork>
        : EFRepositoryNoOC<TEntity, TIdentity, TContext, TUnitOfWork>
        where TEntity : class, IAggregateRoot<TIdentity>
        where TContext : DbContext
        where TUnitOfWork : EFUnitOfWorkBase<TContext>
    {
        public EFRepository(TContext dbContext, TUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        { }

        public override Task UpdateAsync(TEntity entity)
        {
            entity.DateModified = Time.Now;
            return base.UpdateAsync(entity);
        }
    }
}
