using System;
using System.Threading.Tasks;
using LeanCode.Cache;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Cache
{
    public class CacheElement<TContext> : IPipelineElement<TContext, IQuery, object>
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
            IQuery input,
            Func<TContext, IQuery, Task<object>> next)
        {
            var duration = QueryCacheAttribute.GetDuration(input);
            if (duration.HasValue)
            {
                var key = QueryCacheKeyProvider.GetKey(input);
                var res = await cacher
                    .GetOrCreate(key, duration.Value,
                        () => Wrap(ctx, input, next)
                    ).ConfigureAwait(false);
                logger.Debug(
                    "Query result for {@Query}(key: {Key}) retrieved from cache",
                    input, key);

                return res.Item;
            }
            else
            {
                return await next(ctx, input).ConfigureAwait(false);
            }
        }

        private static async Task<CacheItemWrapper> Wrap(
            TContext ctx,
            IQuery query,
            Func<TContext, IQuery, Task<object>> next)
        {
            var result = await next(ctx, query).ConfigureAwait(false);
            return new CacheItemWrapper() { Item = result };
        }

        public sealed class CacheItemWrapper
        {
            public object Item { get; set; }
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, IQuery, object> Cache<TContext>(
            this PipelineBuilder<TContext, IQuery, object> builder)
            where TContext : IPipelineContext
        {
            return builder.Use<CacheElement<TContext>>();
        }
    }
}
