using LeanCode.Components;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore;

public static class ServiceCollectionCQRSExtensions
{
    public static IServiceCollection AddCQRS(this IServiceCollection serviceCollection, TypesCatalog handlersCatalog)
    {
        serviceCollection.AddSingleton<ISerializer, Utf8JsonSerializer>();

        serviceCollection.RegisterGenericTypes(handlersCatalog, typeof(ICommandHandler<,>), ServiceLifetime.Transient);
        serviceCollection.RegisterGenericTypes(handlersCatalog, typeof(IQueryHandler<,,>), ServiceLifetime.Transient);
        serviceCollection.RegisterGenericTypes(handlersCatalog, typeof(IOperationHandler<,,>), ServiceLifetime.Transient);

        return serviceCollection;
    }
}
