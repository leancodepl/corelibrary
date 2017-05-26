using System;
using System.Threading.Tasks;
using LeanCode.Cache;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Security.Exceptions;

namespace LeanCode.CQRS.Default
{
    public class DefaultQueryExecutor : IQueryExecutor
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultQueryExecutor>();

        private readonly IQueryHandlerResolver queryHandlerResolver;
        private readonly ICacher cacher;
        private readonly IQueryCacheKeyProvider keyProvider;
        private readonly IAuthorizer authorizer;

        public DefaultQueryExecutor(
            IQueryHandlerResolver queryHandlerResolver,
            ICacher cacher,
            IQueryCacheKeyProvider keyProvider,
            IAuthorizer authorizer)
        {
            this.queryHandlerResolver = queryHandlerResolver;
            this.cacher = cacher;
            this.keyProvider = keyProvider;
            this.authorizer = authorizer;
        }

        public Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
        {
            logger.Verbose("Executing query {@Query}", query);
            AuthorizeQuery(query);

            var duration = QueryCacheAttribute.GetDuration(query);
            if (!duration.HasValue)
            {
                return JustExecuteQuery(query);
            }
            else
            {
                return LoadFromCache(query, duration);
            }
        }

        private void AuthorizeQuery<TResult>(IQuery<TResult> query)
        {
            switch (authorizer.CheckIfAuthorized(query))
            {
                case AuthorizationResult.InsufficientPermission:
                    logger.Warning("Query {@Query} not authorized", query);
                    throw new InsufficientPermissionException($"User is not authorized for {query.GetType()}.");

                case AuthorizationResult.Unauthenticated:
                    logger.Warning("Query {@Query} requires authorization and user is not authenticated", query);
                    throw new UnauthenticatedException($"User is not authenticated.");
            }
        }

        private async Task<TResult> JustExecuteQuery<TResult>(IQuery<TResult> query)
        {
            var handler = queryHandlerResolver.FindQueryHandler<TResult>(query.GetType());
            if (handler == null)
            {
                logger.Fatal("Cannot find a handler for query {@Query}", query);
                throw new QueryHandlerNotFoundException(query.GetType());
            }
            var result = await handler.ExecuteAsync(query).ConfigureAwait(false);
            logger.Debug("Query {@Query} executed successfuly", query);
            return result;
        }

        private async Task<TResult> LoadFromCache<TResult>(IQuery<TResult> query, TimeSpan? duration)
        {
            var key = keyProvider.GetKey(query);
            var res = await cacher.GetOrCreate(
                key, duration.Value,
                async () => Wrap(await JustExecuteQuery(query).ConfigureAwait(false))
                ).ConfigureAwait(false);
            logger.Debug("Query result for {@Query}(key: {Key}) retrieved from cache", query, key);
            return res.Item;
        }

        private static CacheItemWrapper<TResult> Wrap<TResult>(TResult result)
        {
            return new CacheItemWrapper<TResult>() { Item = result };
        }

        public sealed class CacheItemWrapper<TItem>
        {
            public TItem Item { get; set; }
        }
    }
}
