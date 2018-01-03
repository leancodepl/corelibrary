using System;
using System.Threading.Tasks;
using LeanCode.Cache;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Cache
{
    public class CacheElement<TContext>
        : IPipelineElement<TContext, QueryExecutionPayload, object>
        where TContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CacheElement<TContext>>();

        private readonly ICacher cacher;

        public CacheElement(ICacher cacher)
        {
            this.cacher = cacher;
        }

        public async Task<object> ExecuteAsync(
            TContext ctx,
            QueryExecutionPayload payload,
            Func<TContext, QueryExecutionPayload, Task<object>> next)
        {
            var duration = QueryCacheAttribute.GetDuration(payload.Object);
            if (duration.HasValue)
            {
                var key = QueryCacheKeyProvider.GetKey(payload);
                var res = await cacher
                    .GetOrCreate(key, duration.Value,
                        () => Wrap(ctx, payload, next)
                    ).ConfigureAwait(false);
                logger.Debug(
                    "Query result for {@Query}(key: {Key}) retrieved from cache",
                    payload.Object, key);

                return res.Item;
            }
            else
            {
                return await next(ctx, payload).ConfigureAwait(false);
            }
        }

        private static async Task<CacheItemWrapper> Wrap(
            TContext ctx,
            QueryExecutionPayload payload,
            Func<TContext, QueryExecutionPayload, Task<object>> next)
        {
            var result = await next(ctx, payload).ConfigureAwait(false);
            return new CacheItemWrapper() { Item = result };
        }

        public sealed class CacheItemWrapper
        {
            public object Item { get; set; }
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, QueryExecutionPayload, object> Cache<TContext>(
            this PipelineBuilder<TContext, QueryExecutionPayload, object> builder)
            where TContext : IPipelineContext
        {
            return builder.Use<CacheElement<TContext>>();
        }
    }
}
