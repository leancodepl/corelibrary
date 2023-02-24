using Autofac;
using Autofac.Builder;
using Microsoft.Extensions.Hosting;

namespace LeanCode.PeriodicService;

public static class ContainerBuilderExtensions
{
    public static IRegistrationBuilder<
        PeriodicHostedService<T>,
        ConcreteReflectionActivatorData,
        SingleRegistrationStyle
    > RegisterPeriodicAction<T>(this ContainerBuilder builder)
        where T : IPeriodicAction
    {
        return builder.RegisterType<PeriodicHostedService<T>>().As<IHostedService>();
    }
}
