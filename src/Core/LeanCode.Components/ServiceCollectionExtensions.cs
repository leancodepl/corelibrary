using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.Components;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryRegisterWithImplementedInterfaces<T>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient
    )
        where T : class
    {
        return TryRegisterWithImplementedInterfacesBase<T>(services, lifetime, null);
    }

    public static IServiceCollection TryRegisterWithImplementedInterfaces<T>(
        this IServiceCollection services,
        Func<IServiceProvider, object> implementationFactory,
        ServiceLifetime lifetime = ServiceLifetime.Transient
    )
        where T : class
    {
        return TryRegisterWithImplementedInterfacesBase<T>(services, lifetime, implementationFactory);
    }

    private static IServiceCollection TryRegisterWithImplementedInterfacesBase<T>(
        IServiceCollection services,
        ServiceLifetime lifetime,
        Func<IServiceProvider, object>? implementationFactory
    )
        where T : class
    {
        var implementationType = typeof(T);

        TryAddServiceDescriptor(services, implementationType, implementationType, lifetime, implementationFactory);

        var interfaces = implementationType.GetInterfaces();

        foreach (var interfaceType in interfaces)
        {
            TryAddServiceDescriptor(services, interfaceType, implementationType, lifetime, implementationFactory);
        }

        return services;
    }

    private static void TryAddServiceDescriptor(
        IServiceCollection services,
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime,
        Func<IServiceProvider, object>? implementationFactory
    )
    {
        if (implementationFactory is null)
        {
            services.TryAdd(new ServiceDescriptor(serviceType, implementationType, lifetime));
        }
        else
        {
            services.TryAdd(new ServiceDescriptor(serviceType, implementationFactory, lifetime));
        }
    }
}
