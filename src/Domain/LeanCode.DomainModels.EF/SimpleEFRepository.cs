using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public sealed class SimpleEFRepository<TEntity, TIdentity, TContext>
        : EFRepository<TEntity, TIdentity, TContext>
        where TEntity : class, IAggregateRoot<TIdentity>
        where TIdentity : notnull, IEquatable<TIdentity>
        where TContext : notnull, DbContext
    {
        public SimpleEFRepository(TContext dbContext)
            : base(dbContext) { }

        public override Task<TEntity?> FindAsync(TIdentity id, CancellationToken cancellationToken = default) =>
            DbSet.AsTracking().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }
}
