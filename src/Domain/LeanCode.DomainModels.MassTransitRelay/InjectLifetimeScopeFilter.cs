using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay
{
    // When combining Autofac and Microsoft.Extensions.Dependency injection
    // some DI components are not injected correctly into the message payload,
    // so we have to fix it by ourselves.
    public class InjectLifetimeScopeFilter<T> : IFilter<ConsumeContext<T>>
        where T : class
    {
        public void Probe(ProbeContext context)
        { }

        public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            if (!context.TryGetPayload(out Autofac.Extensions.DependencyInjection.AutofacServiceProvider? scopeProvider))
            {
                throw new InvalidOperationException("Could not find autofac service provider");
            }

            context.GetOrAddPayload(() => scopeProvider!.LifetimeScope);
            return next.Send(context);
        }
    }

    public static class InjectLifetimeScopeFilter
    {
        public static void UseLifetimeScopeInjection(this IConsumePipeConfigurator configurator, IServiceProvider ctx)
        {
            DependencyInjectionFilterExtensions.UseConsumeFilter(
                configurator,
                typeof(InjectLifetimeScopeFilter<>),
                ctx);
        }
    }
}
