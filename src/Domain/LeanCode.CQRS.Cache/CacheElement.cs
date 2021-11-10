using System;
using System.Threading.Tasks;
using LeanCode.Cache;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Cache
{
    public class CacheElement<TAppContext>
        : IPipelineElement<TAppContext, IQuery, object?>
        where TAppContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CacheElement<TAppContext>>();

        private readonly ICacher cacher;

        public CacheElement(ICacher cacher)
        {
            this.cacher = cacher;
        }

        public async Task<object?> ExecuteAsync(
            TAppContext ctx,
            IQuery input,
            Func<TAppContext, IQuery, Task<object?>> next)
        {
            if (QueryCacheAttribute.GetDuration(input) is TimeSpan duration)
            {
                var key = QueryCacheKeyProvider.GetKey(ctx, input);

                var res = await cacher.GetOrCreateAsync(key, duration, () => WrapAsync(ctx, input, next));

                logger.Debug(
                    "Query result for {@Query}(key: {Key}) retrieved from cache",
                    input, key);

                return res.Item;
            }
            else
            {
                return await next(ctx, input);
            }
        }

        private static async Task<CacheItemWrapper> WrapAsync(
            TAppContext ctx,
            IQuery payload,
            Func<TAppContext, IQuery, Task<object?>> next)
        {
            var result = await next(ctx, payload);

            return new CacheItemWrapper(result);
        }

        private sealed class CacheItemWrapper
        {
            public object? Item { get; set; }

            public CacheItemWrapper(object? item)
            {
                Item = item;
            }
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, IQuery, object?> Cache<TContext>(
            this PipelineBuilder<TContext, IQuery, object?> builder)
            where TContext : IPipelineContext
        {
            return builder.Use<CacheElement<TContext>>();
        }
    }
}
