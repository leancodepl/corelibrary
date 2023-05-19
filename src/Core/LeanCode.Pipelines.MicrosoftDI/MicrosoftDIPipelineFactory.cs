using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Pipelines.MicrosoftDI;

public class MicrosoftDIPipelineFactory : IPipelineFactory
{
    private readonly IServiceScopeFactory serviceScopeFactory;

    public MicrosoftDIPipelineFactory(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public IPipelineScope BeginScope() => new PipelineScope(serviceScopeFactory.CreateScope());

    private sealed class PipelineScope : IPipelineScope
    {
        private readonly IServiceScope scope;

        public PipelineScope(IServiceScope scope)
        {
            this.scope = scope;
        }

        public IPipelineElement<TContext, TInput, TOutput> ResolveElement<TContext, TInput, TOutput>(Type type)
            where TContext : IPipelineContext
        {
            return (IPipelineElement<TContext, TInput, TOutput>)scope.ServiceProvider.GetRequiredService(type);
        }

        public IPipelineFinalizer<TContext, TInput, TOutput> ResolveFinalizer<TContext, TInput, TOutput>(Type type)
            where TContext : IPipelineContext
        {
            return (IPipelineFinalizer<TContext, TInput, TOutput>)scope.ServiceProvider.GetRequiredService(type);
        }

        public void Dispose() => scope.Dispose();
    }
}
