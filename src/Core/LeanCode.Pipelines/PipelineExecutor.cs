using System;
using System.Threading.Tasks;

namespace LeanCode.Pipelines
{
    public static class PipelineExecutor
    {
        public static PipelineExecutor<TContext, TInput, TOutput> Create<TContext, TInput, TOutput>(
            IPipelineFactory factory,
            ConfiguredPipeline<TContext, TInput, TOutput> cfg)
            where TContext : IPipelineContext
        {
            return new PipelineExecutor<TContext, TInput, TOutput>(factory, cfg);
        }
    }

    public class PipelineExecutor<TContext, TInput, TOutput>
        where TContext : IPipelineContext
    {
        private readonly IPipelineFactory factory;
        private readonly Func<TContext, TInput, Task<TOutput>> exec;

        public PipelineExecutor(
            IPipelineFactory factory,
            ConfiguredPipeline<TContext, TInput, TOutput> config)
        {
            this.factory = factory;
            this.exec = BuildPipeline(config);
        }

        public async Task<TOutput> ExecuteAsync(TContext ctx, TInput input)
        {
            using var scope = factory.BeginScope();

            ctx.Scope = scope;

            return await exec(ctx, input).ConfigureAwait(false);
        }

        private static Func<TContext, TInput, Task<TOutput>> BuildPipeline(
            ConfiguredPipeline<TContext, TInput, TOutput> config)
        {
            var app = BuildNext(config.Finalizer);

            for (int i = config.Elements.Count - 1; i >= 0; i--)
            {
                app = BuildNext(config.Elements[i], app);
            }

            return app;
        }

        private static Func<TContext, TInput, Task<TOutput>> BuildNext(Type finalType)
        {
            return (ctx, input) => ctx.Scope
                .ResolveFinalizer<TContext, TInput, TOutput>(finalType)
                .ExecuteAsync(ctx, input);
        }

        private static Func<TContext, TInput, Task<TOutput>> BuildNext(
            Type pipelineType,
            Func<TContext, TInput, Task<TOutput>> next)
        {
            return (ctx, input) => ctx.Scope
                .ResolveElement<TContext, TInput, TOutput>(pipelineType)
                .ExecuteAsync(ctx, input, next);
        }
    }
}
