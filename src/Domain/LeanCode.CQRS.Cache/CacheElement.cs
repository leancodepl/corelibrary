using System;
using System.Threading.Tasks;
using LeanCode.Cache;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Cache
{
    public class CacheElement : IPipelineElement<ExecutionContext, IQuery, object>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CacheElement>();

        private readonly ICacher cacher;

        public CacheElement(ICacher cacher)
        {
            this.cacher = cacher;
        }

        public async Task<object> ExecuteAsync(
            ExecutionContext ctx,
            IQuery input,
            Func<ExecutionContext, IQuery, Task<object>> next)
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
            ExecutionContext ctx,
            IQuery query,
            Func<ExecutionContext, IQuery, Task<object>> next)
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
        public static PipelineBuilder<ExecutionContext, IQuery, object> Cache(
            this PipelineBuilder<ExecutionContext, IQuery, object> builder)
        {
            return builder.Use<CacheElement>();
        }
    }
}
