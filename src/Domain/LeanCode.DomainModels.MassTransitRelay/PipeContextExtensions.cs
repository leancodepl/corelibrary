using System;
using Autofac;
using GreenPipes;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public static class PipeContextExtensions
    {
        public static TService GetService<TService>(this PipeContext pipe)
        {
            if (!pipe.TryGetPayload<ILifetimeScope>(out var scope))
            {
                throw new InvalidOperationException("Cannot resolve service. ILifeTimeScope not available for pipe context");
            }

            return scope.Resolve<TService>();
        }
    }
}
