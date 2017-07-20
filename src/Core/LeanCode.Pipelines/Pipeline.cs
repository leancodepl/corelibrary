using System;
using System.Collections.Generic;

namespace LeanCode.Pipelines
{
    public static class Pipeline
    {
        public static PipelineBuilder<TInput, TOutput> Build<TInput, TOutput>()
            => new PipelineBuilder<TInput, TOutput>();
    }

    public class ConfiguredPipeline<TInput, TOutput>
    {
        public IReadOnlyList<Type> Elements { get; }
        public Type Finalizer { get; }

        public ConfiguredPipeline(
            IReadOnlyList<Type> elements,
            Type finalElement)
        {
            Elements = elements;
            Finalizer = finalElement;
        }
    }

    public class PipelineBuilder<TInput, TOutput>
    {
        private readonly List<Type> components = new List<Type>();

        public PipelineBuilder<TInput, TOutput> Use<TPipeline>()
            where TPipeline : class, IPipelineElement<TInput, TOutput>
        {
            components.Add(typeof(TPipeline));
            return this;
        }

        public ConfiguredPipeline<TInput, TOutput> Finalize<TPipeline>()
            where TPipeline : class, IPipelineFinalizer<TInput, TOutput>
        {
            return new ConfiguredPipeline<TInput, TOutput>(
                components,
                typeof(TPipeline)
            );
        }
    }
}
