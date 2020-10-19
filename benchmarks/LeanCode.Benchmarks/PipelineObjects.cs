using System;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.Benchmarks.Pipelines
{
    public struct Context : IPipelineContext
    {
        public IPipelineScope Scope { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

    public struct Input { }
    public struct Output { }

    public class Finalizer : IPipelineFinalizer<Context, Input, Output>
    {
        private readonly Task<Output> result = Task.FromResult(new Output());
        public Task<Output> ExecuteAsync(Context ctx, Input input) => result;
    }

    public class PassthroughElement : IPipelineElement<Context, Input, Output>
    {
        public Task<Output> ExecuteAsync(
            Context ctx,
            Input input,
            Func<Context, Input, Task<Output>> next)
        {
            return next(ctx, input);
        }
    }

    public class StaticScope : IPipelineScope
    {
        private readonly PassthroughElement element = new PassthroughElement();
        private readonly Finalizer finalizer = new Finalizer();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public IPipelineElement<TContext, TInput, TOutput> ResolveElement<TContext, TInput, TOutput>(Type type)
            where TContext : IPipelineContext
        {
            return (IPipelineElement<TContext, TInput, TOutput>)(object)element;
        }

        public IPipelineFinalizer<TContext, TInput, TOutput> ResolveFinalizer<TContext, TInput, TOutput>(Type type)
            where TContext : IPipelineContext
        {
            return (IPipelineFinalizer<TContext, TInput, TOutput>)(object)finalizer;
        }
    }

    public class StaticFactory : IPipelineFactory
    {
        private readonly StaticScope scope = new StaticScope();
        public IPipelineScope BeginScope() => scope;
    }
}
