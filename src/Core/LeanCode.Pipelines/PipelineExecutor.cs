using System;
using System.Threading.Tasks;

namespace LeanCode.Pipelines
{
    public static class PipelineExecutor
    {
        public static PipelineExecutor<TInput, TOutput> Create<TInput, TOutput>(
            IPipelineFactory factory,
            ConfiguredPipeline<TInput, TOutput> cfg)
        {
            return new PipelineExecutor<TInput, TOutput>(factory, cfg);
        }
    }

    public class PipelineExecutor<TInput, TOutput>
    {
        private readonly IPipelineFactory factory;
        private readonly Func<IPipelineScope, TInput, Task<TOutput>> exec;

        public PipelineExecutor(
            IPipelineFactory factory,
            ConfiguredPipeline<TInput, TOutput> config)
        {
            this.factory = factory;
            this.exec = BuildPipeline(config);
        }

        public async Task<TOutput> ExecuteAsync(TInput input)
        {
            using (var scope = factory.BeginScope())
            {
                return await exec(scope, input).ConfigureAwait(false);
            }
        }

        private static Func<IPipelineScope, TInput, Task<TOutput>> BuildPipeline(
            ConfiguredPipeline<TInput, TOutput> config)
        {
            var app = BuildNext(config.Finalizer);
            for (int i = config.Elements.Count - 1; i >= 0; i--)
            {
                var pipelineType = config.Elements[i];
                app = BuildNext(pipelineType, app);
            }
            return app;
        }

        private static Func<IPipelineScope, TInput, Task<TOutput>> BuildNext(
            Type finalType)
        {
            return (scope, input) =>
            {
                var final = scope.ResolveFinalizer<TInput, TOutput>(finalType);
                return final.ExecuteAsync(input);
            };
        }

        private static Func<IPipelineScope, TInput, Task<TOutput>> BuildNext(
            Type pipelineType,
            Func<IPipelineScope, TInput, Task<TOutput>> next
        )
        {
            return (scope, input) =>
            {
                var element = scope.ResolveElement<TInput, TOutput>(pipelineType);
                return element.ExecuteAsync(input, input2 => next(scope, input2));
            };
        }
    }
}
