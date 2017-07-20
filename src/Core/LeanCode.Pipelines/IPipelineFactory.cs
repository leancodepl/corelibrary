using System;

namespace LeanCode.Pipelines
{
    public interface IPipelineFactory
    {
        IPipelineScope BeginScope();
    }

    public interface IPipelineScope : IDisposable
    {
        IPipelineElement<TInput, TOutput> ResolveElement<TInput, TOutput>(Type type);
        IPipelineFinalizer<TInput, TOutput> ResolveFinalizer<TInput, TOutput>(Type type);
    }
}
