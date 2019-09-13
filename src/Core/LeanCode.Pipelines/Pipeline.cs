using System;
using System.Collections.Generic;

namespace LeanCode.Pipelines
{
    public static class Pipeline
    {
        public static PipelineBuilder<TContext, TInput, TOutput> Build<TContext, TInput, TOutput>()
            where TContext : notnull, IPipelineContext
        {
            return new PipelineBuilder<TContext, TInput, TOutput>();
        }
    }

    public class ConfiguredPipeline<TContext, TInput, TOutput>
        where TContext : notnull, IPipelineContext
    {
        public IReadOnlyList<Type> Elements { get; }
        public Type Finalizer { get; }

        public ConfiguredPipeline(IReadOnlyList<Type> elements, Type finalElement)
        {
            Elements = elements;
            Finalizer = finalElement;
        }
    }

    public delegate PipelineBuilder<TContext, TInput, TOutput> ConfigPipeline<TContext, TInput, TOutput>(
        PipelineBuilder<TContext, TInput, TOutput> builder)
            where TContext : notnull, IPipelineContext;

    public class PipelineBuilder<TContext, TInput, TOutput>
        where TContext : notnull, IPipelineContext
    {
        private readonly List<Type> components = new List<Type>();

        public PipelineBuilder<TContext, TInput, TOutput> Use<TPipeline>()
            where TPipeline : class, IPipelineElement<TContext, TInput, TOutput>
        {
            components.Add(typeof(TPipeline));
            return this;
        }

        public PipelineBuilder<TContext, TInput, TOutput> Configure(
            ConfigPipeline<TContext, TInput, TOutput> config)
        {
            return config(this);
        }

        public ConfiguredPipeline<TContext, TInput, TOutput> Finalize<TPipeline>()
            where TPipeline : class, IPipelineFinalizer<TContext, TInput, TOutput>
        {
            return new ConfiguredPipeline<TContext, TInput, TOutput>(
                components,
                typeof(TPipeline));
        }
    }
}
