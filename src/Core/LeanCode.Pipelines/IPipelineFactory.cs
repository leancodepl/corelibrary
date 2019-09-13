using System;

namespace LeanCode.Pipelines
{
    public interface IPipelineFactory
    {
        IPipelineScope BeginScope();
    }

    public interface IPipelineScope : IDisposable
    {
        IPipelineElement<TContext, TInput, TOutput> ResolveElement<TContext, TInput, TOutput>(Type type)
            where TContext : notnull, IPipelineContext;

        IPipelineFinalizer<TContext, TInput, TOutput> ResolveFinalizer<TContext, TInput, TOutput>(Type type)
            where TContext : notnull, IPipelineContext;
    }
}
