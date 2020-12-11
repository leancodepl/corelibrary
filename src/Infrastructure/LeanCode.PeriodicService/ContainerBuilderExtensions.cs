using Autofac;
using Autofac.Builder;
using LeanCode.OrderedHostedServices;

namespace LeanCode.PeriodicService
{
    public static class ContainerBuilderExtensions
    {
        public static IRegistrationBuilder<PeriodicHostedService<T>, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterPeriodicAction<T>(this ContainerBuilder builder, int order)
            where T : IPeriodicAction
        {
            return builder.RegisterOrderedHostedService<PeriodicHostedService<T>>()
                .WithParameter(nameof(order), order);
        }

        public static IRegistrationBuilder<PeriodicHostedService<T>, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterPeriodicAction<T>(this ContainerBuilder builder)
            where T : IPeriodicAction
        {
            return builder.RegisterPeriodicAction<T>(order: 0);
        }
    }
}
