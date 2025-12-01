using System.Collections.Concurrent;
using LeanCode.Logging;

namespace LeanCode.ViewRenderer.Razor;

internal class CompiledViewsCache
{
    private readonly ViewLocator locator;
    private readonly ViewCompiler compiler;
    private readonly ILogger<CompiledViewsCache> logger;

    private readonly ConcurrentDictionary<string, CompiledView> cache =
        new ConcurrentDictionary<string, CompiledView>();

    private readonly ConcurrentDictionary<string, TaskCompletionSource<CompiledView>> buildCache =
        new ConcurrentDictionary<string, TaskCompletionSource<CompiledView>>();

    public CompiledViewsCache(RazorViewRendererOptions opts, ILogger<CompiledViewsCache> logger)
    {
        locator = new(opts, logger.ForContext<ViewLocator>());
        compiler = new(locator, logger.ForContext<ViewCompiler>());
        this.logger = logger;
    }

    public ValueTask<CompiledView> GetOrCompileAsync(string viewName)
    {
        if (cache.TryGetValue(viewName, out var compiled))
        {
            logger.Verbose("View type for {ViewName} retrieved from cache", viewName);

            return new ValueTask<CompiledView>(compiled);
        }

        logger.Verbose("View type for {ViewName} is not in cache, compiling", viewName);

        var tcs = new TaskCompletionSource<CompiledView>();

        var newTcs = buildCache.GetOrAdd(viewName, tcs);

        if (tcs == newTcs)
        {
            // Double-check the main cache. This handles the race condition where:
            // 1. We checked cache above → miss
            // 2. Another thread finished compilation, added to cache, and removed from buildCache
            // 3. We added our TCS to buildCache (which was now empty)
            // Without this check, we'd start a redundant compilation.
            if (cache.TryGetValue(viewName, out compiled))
            {
                logger.Verbose(
                    "View type for {ViewName} was added to cache while we were setting up compilation",
                    viewName
                );
                // Complete the TCS before removing - other threads may be waiting on it
                tcs.SetResult(compiled);
                buildCache.TryRemove(viewName, out _);
                return new ValueTask<CompiledView>(compiled);
            }

            return new ValueTask<CompiledView>(WrappedCompileAsync(viewName, tcs));
        }

        logger.Verbose("View type for {ViewName} is being compiled, waiting", viewName);

        return new ValueTask<CompiledView>(newTcs.Task);
    }

    private async Task<CompiledView> WrappedCompileAsync(string viewName, TaskCompletionSource<CompiledView> tcs)
    {
        try
        {
            var result = await CompileAsync(viewName);

            // The order is _very_ important - we need to store the result so that
            // other tasks won't wait on the TCS (perf), then we need to set the TCS result (perf),
            // and at the very end we need to remove the TCS from cache (to prevent race condition).
            cache.TryAdd(viewName, result);
            tcs.SetResult(result);
            buildCache.TryRemove(viewName, out var _);

            return result;
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);

            // Removing the TCS from cache will allow to fix & recompile views without app restart.
            buildCache.TryRemove(viewName, out var _);

            throw;
        }
    }

    private async Task<CompiledView> CompileAsync(string viewName)
    {
        var item = locator.GetItem(viewName, null);
        if (!item.Exists)
        {
            logger.Debug("Cannot locate view {ViewName}", viewName);
            throw new ViewNotFoundException(viewName, "Cannot locate view.");
        }

        logger.Information(
            "View {ViewName} located at {ViewPath}, running real compilation",
            viewName,
            item.PhysicalPath
        );
        return await compiler.CompileAsync(item);
    }
}
