using System;
using Autofac;

namespace LeanCode.Pipelines.Autofac
{
    public class AutofacPipelineFactory : IPipelineFactory
    {
        private readonly ILifetimeScope scope;

        public AutofacPipelineFactory(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public IPipelineScope BeginScope()
        {
            return new PipelineScope(scope.BeginLifetimeScope());
        }

        private sealed class PipelineScope : IPipelineScope
        {
            private readonly ILifetimeScope scope;

            public PipelineScope(ILifetimeScope scope)
            {
                this.scope = scope;
            }

            public IPipelineElement<TInput, TOutput>
                ResolveElement<TInput, TOutput>(Type type)
            {
                return (IPipelineElement<TInput, TOutput>)scope.Resolve(type);
            }

            public IPipelineFinalizer<TInput, TOutput>
                ResolveFinalizer<TInput, TOutput>(Type type)
            {
                return (IPipelineFinalizer<TInput, TOutput>)scope.Resolve(type);
            }

            public void Dispose()
            {
                scope.Dispose();
            }
        }
    }
}
