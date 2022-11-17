using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.DataAccess
{
    public static class IRepositoryExtensions
    {
        public static async Task<TEntity> FindAndEnsureExistsAsync<TEntity, TIdentity>(
            this IRepository<TEntity, TIdentity> repository,
            TIdentity id,
            CancellationToken cancellationToken = default)
            where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
            where TIdentity : notnull
        {
            var entity = await repository.FindAsync(id, cancellationToken);
            return entity ?? throw new EntityDoesNotExistException(typeof(TEntity), id.ToString());
        }
    }

    public class EntityDoesNotExistException : ArgumentNullException
    {
        public Type EntityType { get; }
        public string EntityId { get; }

        public EntityDoesNotExistException(Type entityType, string? entityId)
            : base($"Aggregate of type: {entityType.Name} with id: {entityId} does not exist.")
        {
            EntityType = entityType;
            EntityId = entityId ?? "";
        }
    }
}
