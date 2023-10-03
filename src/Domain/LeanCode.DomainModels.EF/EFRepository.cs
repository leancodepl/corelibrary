using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

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
    /// <param name="id">The identifier that</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Task with the found entity, or -null- if not found.</returns>
    public virtual async Task<TEntity?> FindAsync(TIdentity id, CancellationToken cancellationToken = default)
    {
        return await BaseQuery().AsTracking().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }

    [SuppressMessage(
        "?",
        "EF1001",
        Justification = "This is basically the `EntityFinder.FindTracked` to mimic `FindAsync` snapshot re-use."
    )]
    protected TEntity? FindTracked(TIdentity id)
    {
        // Safety: aggregates are bound to have Id as a primary key by design.
        var primaryKey = DbContext.Model.FindEntityType(typeof(TEntity))!.FindPrimaryKey()!;
        return ((IDbContextDependencies)DbContext).StateManager!.TryGetEntryTyped(primaryKey, id)?.Entity as TEntity;
    }

    [SuppressMessage(
        "?",
        "EF1001",
        Justification = "This is basically the `EntityFinder.FindTracked` to mimic `FindAsync` snapshot re-use."
    )]
    protected TEntity? FindTracked(params object[] id)
    {
        // Safety: aggregates are bound to have Id as a primary key by design.
        var primaryKey = DbContext.Model.FindEntityType(typeof(TEntity))!.FindPrimaryKey()!;
        return ((IDbContextDependencies)DbContext).StateManager!.TryGetEntryTyped(primaryKey, id)?.Entity as TEntity;
    }

    protected ValueTask<TEntity?> FindTrackedOrLoadNewAsync(TIdentity id, Func<DbSet<TEntity>, Task<TEntity?>> query)
    {
        var tracked = FindTracked(id);
        return tracked is not null ? new(tracked) : new(query(DbSet));
    }

    protected ValueTask<TEntity?> FindTrackedOrLoadNewAsync(object[] id, Func<DbSet<TEntity>, Task<TEntity?>> query)
    {
        var tracked = FindTracked(id);
        return tracked is not null ? new(tracked) : new(query(DbSet));
    }
}
