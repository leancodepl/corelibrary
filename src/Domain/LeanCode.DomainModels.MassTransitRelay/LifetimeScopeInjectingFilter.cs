using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using GreenPipes;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class LifetimeScopeInjectingFilter : IFilter<ConsumeContext>
    {
        private readonly Func<ILifetimeScope> scope;

        public LifetimeScopeInjectingFilter(Func<ILifetimeScope> scope)
        {
            this.scope = scope;
        }

        public void Probe(ProbeContext context)
        {
        }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            using (var scope = this.scope().BeginLifetimeScope())
            {
                context.GetOrAddPayload(() => scope);
                await next.Send(context);
            }
        }
    }

    public static class LifetimeScopeInjectingFilterExtensions
    {
        public static void UseLifetimeScopeInjection(this IPipeConfigurator<ConsumeContext> config, Func<ILifetimeScope> scope)
        {
            config.AddPipeSpecification(new LifetimeScopeInjectingFilterPipeSpefication(scope));
        }
    }

    public class LifetimeScopeInjectingFilterPipeSpefication : IPipeSpecification<ConsumeContext>
    {
        private readonly Func<ILifetimeScope> scope;

        public LifetimeScopeInjectingFilterPipeSpefication(Func<ILifetimeScope> scope)
        {
            this.scope = scope;
        }

        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new LifetimeScopeInjectingFilter(scope));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }
}
