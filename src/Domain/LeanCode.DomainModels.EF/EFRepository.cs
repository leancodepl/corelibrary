using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF;

public abstract class EFRepository<TEntity, TIdentity, TContext> : IRepository<TEntity, TIdentity>
    where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
    where TIdentity : notnull
    where TContext : notnull, DbContext
{
    protected TContext DbContext { get; }
    protected DbSet<TEntity> DbSet { get; }

    protected EFRepository(TContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual void Add(TEntity entity)
    {
        if (entity is IOptimisticConcurrency oc)
        {
            oc.DateModified = Time.Now;
        }

        DbSet.Add(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        if (entity is IOptimisticConcurrency oc)
        {
            oc.DateModified = Time.Now;
        }

        DbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (var oc in entities.OfType<IOptimisticConcurrency>())
        {
            oc.DateModified = Time.Now;
        }

        DbSet.RemoveRange(entities);
    }

    public virtual void Update(TEntity entity)
    {
        if (entity is IOptimisticConcurrency oc)
        {
            oc.DateModified = Time.Now;
        }
    }

    public abstract Task<TEntity?> FindAsync(TIdentity id, CancellationToken cancellationToken = default);
}
