using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.ViewRenderer.Razor
{
    class CompiledViewsCache
    {
        private readonly ViewLocator locator;
        private readonly ViewCompiler compiler;

        private readonly ConcurrentDictionary<string, CompiledView> cache =
            new ConcurrentDictionary<string, CompiledView>();

        private readonly ConcurrentDictionary<string, TaskCompletionSource<CompiledView>> buildCache =
            new ConcurrentDictionary<string, TaskCompletionSource<CompiledView>>();

        public CompiledViewsCache(RazorViewRendererOptions opts)
        {
            locator = new ViewLocator(opts);
            compiler = new ViewCompiler();
        }

        public ValueTask<CompiledView> GetOrCompile(string viewName)
        {
            if (cache.TryGetValue(viewName, out var compiled))
            {
                return new ValueTask<CompiledView>(compiled);
            }
            else
            {
                var tcs = new TaskCompletionSource<CompiledView>();
                var newTcs = buildCache.GetOrAdd(viewName, tcs);
                if (tcs == newTcs)
                {
                    return new ValueTask<CompiledView>(WrappedCompile(viewName, tcs));
                }
                else
                {
                    return new ValueTask<CompiledView>(newTcs.Task);
                }
            }
        }
        private async Task<CompiledView> WrappedCompile(string viewName, TaskCompletionSource<CompiledView> tcs)
        {
            try
            {
                var result = await Compile(viewName).ConfigureAwait(false);

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

        private async Task<CompiledView> Compile(string viewName)
        {
            var viewLocation = locator.LocateView(viewName);
            if (viewLocation == null)
            {
                throw new ViewNotFoundException(viewName, "Cannot locate view.");
            }

            return await compiler.Compile(viewLocation).ConfigureAwait(false);
        }
    }
}
