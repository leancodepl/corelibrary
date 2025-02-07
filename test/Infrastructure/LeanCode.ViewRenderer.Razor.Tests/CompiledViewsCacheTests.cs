using LeanCode.Test.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LeanCode.ViewRenderer.Razor.Tests;

public class CompiledViewsCacheTests
{
    private const string View = "Simple";
    private static readonly RazorViewRendererOptions Options = new RazorViewRendererOptions("./Views/Cache");

    private readonly CompiledViewsCache cache;

    public CompiledViewsCacheTests()
    {
        var compiledViewsCacheLogger = Substitute.For<ILogger<CompiledViewsCache>>();
        var viewLocatorLogger = Substitute.For<ILogger<ViewLocator>>();
        var viewCompilerLogger = Substitute.For<ILogger<ViewCompiler>>();

        cache = new CompiledViewsCache(compiledViewsCacheLogger, viewLocatorLogger, viewCompilerLogger, Options);
    }

    [Fact]
    public async Task Loads_the_view_if_not_in_cache()
    {
        var result = await cache.GetOrCompileAsync(View);

        Assert.NotNull(result.ViewType);
    }

    [Fact]
    public async Task Loads_the_view_from_cache_if_already_loaded()
    {
        var result = await cache.GetOrCompileAsync(View);
        var result2 = await cache.GetOrCompileAsync(View);

        Assert.Same(result.ViewType, result2.ViewType);
    }

    [Fact]
    public async Task Correctly_compiles_the_views_when_there_are_multiple_symultaneous_calls()
    {
        var t1 = cache.GetOrCompileAsync(View);
        var t2 = cache.GetOrCompileAsync(View);

        var results = await Task.WhenAll(t1.AsTask(), t2.AsTask());

        Assert.Same(results[0].ViewType, results[1].ViewType);
    }

    [LongRunningFact]
    public async Task Stress_test_the_multithreading_part()
    {
        for (var i = 0; i < 10; i++)
        {
            var tasks = Enumerable.Range(0, 1000).Select(_ => cache.GetOrCompileAsync(View).AsTask());
            var results = await Task.WhenAll(tasks);

            var expected = results[0].ViewType;
            foreach (var r in results)
            {
                Assert.Same(expected, r.ViewType);
            }
        }
    }
}
