using System.Linq;
using System.Threading.Tasks;
using LeanCode.Test.Helpers;
using Xunit;

namespace LeanCode.ViewRenderer.Razor.Tests
{
    public class CompiledViewsCacheTests
    {
        const string View = "Simple";
        private static readonly RazorViewRendererOptions Options = new RazorViewRendererOptions("./Views/Cache");

        private readonly CompiledViewsCache cache;

        public CompiledViewsCacheTests()
        {
            cache = new CompiledViewsCache(Options);
        }

        [Fact]
        public async Task Loads_the_view_if_not_in_cache()
        {
            var result = await cache.GetOrCompile(View);

            Assert.NotNull(result.ViewType);
        }

        [Fact]
        public async Task Loads_the_view_from_cache_if_already_loaded()
        {
            var result = await cache.GetOrCompile(View);
            var result2 = await cache.GetOrCompile(View);

            Assert.Same(result.ViewType, result2.ViewType);
        }

        [Fact]
        public async Task Correctly_compiles_the_views_when_there_are_multiple_symultaneous_calls()
        {
            var t1 = cache.GetOrCompile(View);
            var t2 = cache.GetOrCompile(View);

            var results = await Task.WhenAll(t1.AsTask(), t2.AsTask());

            Assert.Same(results[0].ViewType, results[1].ViewType);
        }

        [LongRunningFact]
        public async Task Stress_test_the_multithreading_part()
        {
            for (int i = 0; i < 10; i++)
            {
                var tasks = Enumerable.Range(0, 1000).Select(_ => cache.GetOrCompile(View).AsTask());
                var results = await Task.WhenAll(tasks);

                var expected = results[0].ViewType;
                foreach (var r in results)
                {
                    Assert.Same(expected, r.ViewType);
                }
            }
        }
    }
}
