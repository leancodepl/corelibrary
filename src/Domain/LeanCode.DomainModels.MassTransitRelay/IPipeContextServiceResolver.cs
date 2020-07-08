using System;
using Autofac;
using GreenPipes;

namespace LeanCode.DomainModels.MassTransitRelay
{
    /// <remarks>
    /// This interface provides a testable abstraction over service locator pattern used in filters
    /// </remarks>
    public interface IPipeContextServiceResolver
    {
        TService GetService<TService>(PipeContext pipe)
            where TService : notnull;
    }

    public class AutofacPipeContextServiceResolver : IPipeContextServiceResolver
    {
        public static readonly AutofacPipeContextServiceResolver Instance = new AutofacPipeContextServiceResolver();

        public TService GetService<TService>(PipeContext pipe)
            where TService : notnull
        {
            if (pipe.TryGetPayload<ILifetimeScope>(out var scope))
            {
                return scope.Resolve<TService>();
            }
            else
            {
                throw new InvalidOperationException("Cannot resolve service. ILifetimeScope not available for pipe context.");
            }
        }
    }
}
