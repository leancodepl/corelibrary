using System.Diagnostics.CodeAnalysis;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace LeanCode.DomainModels.EF;

public abstract class CachingEFRepository<TEntity, TIdentity, TContext> : EFRepository<TEntity, TIdentity, TContext>
    where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
    where TIdentity : notnull
    where TContext : DbContext
{
    protected CachingEFRepository(TContext dbContext)
        : base(dbContext) { }

    /// <summary>
    /// Finds an entity by primary key. If the entity with provided key is tracked by the underlying
    /// <see cref="DbContext" />, this method will return the cached entity.
    /// </summary>
    /// <remarks>For implementers: the default implementation won't work for composite primary keys.</remarks>
    /// <param name="id">The identifier that</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Task with the found entity, or -null- if not found.</returns>
    public override async Task<TEntity?> FindAsync(TIdentity id, CancellationToken cancellationToken = default)
    {
        return await FindTrackedOrLoadNewAsync(
            id,
            _ => BaseQuery().AsTracking().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken)
        );
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
