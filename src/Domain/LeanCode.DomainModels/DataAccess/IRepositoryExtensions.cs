using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.DataAccess
{
    public static class IRepositoryExtensions
    {
        public static async Task<TEntity> FindAndEnsureExistsAsync<TEntity, TIdentity>(this IRepository<TEntity, TIdentity> repository, TIdentity id)
            where TEntity : class, IAggregateRootWithoutOptimisticConcurrency<TIdentity>
            where TIdentity : notnull
        {
            var entity = await repository.FindAsync(id);
            return entity ?? throw new ArgumentException($"Aggregate of type: {typeof(TEntity).Name} with id: {id} does not exist");
        }
    }
}
