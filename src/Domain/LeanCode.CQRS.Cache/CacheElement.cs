using System;
using System.Threading.Tasks;
using LeanCode.Cache;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Cache
{
    public class CacheElement : IPipelineElement<IQuery, object>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CacheElement>();

        private readonly ICacher cacher;

        public CacheElement(ICacher cacher)
        {
            this.cacher = cacher;
        }

        public async Task<object> ExecuteAsync(
            IQuery input,
            Func<IQuery, Task<object>> next)
        {
            var duration = QueryCacheAttribute.GetDuration(input);
            if (duration.HasValue)
            {
                var key = QueryCacheKeyProvider.GetKey(input);
                var res = await cacher
                    .GetOrCreate(key, duration.Value,
                        async () => Wrap(await next(input).ConfigureAwait(false))
                    ).ConfigureAwait(false);
                logger.Debug(
                    "Query result for {@Query}(key: {Key}) retrieved from cache",
                    input, key);

                return res.Item;
            }
            else
            {
                return await next(input).ConfigureAwait(false);
            }
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

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<IQuery, object> Cache(
            this PipelineBuilder<IQuery, object> builder
        )
        {
            return builder.Use<CacheElement>();
        }
    }
}
