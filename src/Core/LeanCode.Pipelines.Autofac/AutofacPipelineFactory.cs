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

            public IPipelineElement<TContext, TInput, TOutput>
                ResolveElement<TContext, TInput, TOutput>(Type type)
                where TContext : IPipelineContext
            {
                return (IPipelineElement<TContext, TInput, TOutput>)scope.Resolve(type);
            }

            public IPipelineFinalizer<TContext, TInput, TOutput>
                ResolveFinalizer<TContext, TInput, TOutput>(Type type)
                where TContext : IPipelineContext
            {
                return (IPipelineFinalizer<TContext, TInput, TOutput>)scope.Resolve(type);
            }

            public void Dispose()
            {
                scope.Dispose();
            }
        }
    }
}
