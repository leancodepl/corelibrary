using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF;

public abstract class EFRepository<TEntity, TIdentity, TContext> : IRepository<TEntity, TIdentity>
    where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
    where TIdentity : notnull
    where TContext : DbContext
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
            oc.DateModified = Time.UtcNow;
        }

        DbSet.Add(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        if (entity is IOptimisticConcurrency oc)
        {
            oc.DateModified = Time.UtcNow;
        }

        DbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (var oc in entities.OfType<IOptimisticConcurrency>())
        {
            oc.DateModified = Time.UtcNow;
        }

        DbSet.RemoveRange(entities);
    }

    public virtual void Update(TEntity entity)
    {
        if (entity is IOptimisticConcurrency oc)
        {
            oc.DateModified = Time.UtcNow;
        }
    }

    protected virtual IQueryable<TEntity> BaseQuery() => DbSet.AsQueryable();

    /// <summary>
    /// Finds an entity by primary key.
    /// </summary>
    /// <remarks>For implementers: the default implementation won't work for composite primary keys.</remarks>
    /// <param name="id">The identifier of the aggregate.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Task with the found entity, or -null- if not found.</returns>
    public virtual async Task<TEntity?> FindAsync(TIdentity id, CancellationToken cancellationToken = default)
    {
        return await BaseQuery().AsTracking().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }
}
