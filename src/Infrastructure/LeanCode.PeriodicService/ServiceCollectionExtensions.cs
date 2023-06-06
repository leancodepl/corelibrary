using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.PeriodicService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPeriodicAction<T>(this IServiceCollection builder)
        where T : class, IPeriodicAction
    {
        builder.AddTransient<T, T>();
        return builder.AddHostedService<PeriodicHostedService<T>>();
    }
}
