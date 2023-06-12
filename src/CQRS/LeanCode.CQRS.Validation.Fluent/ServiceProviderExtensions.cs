using FluentValidation;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Validation.Fluent;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddFluentValidation(
        this IServiceCollection serviceCollection,
        TypesCatalog catalog,
        ServiceLifetime validatorsLifetime = ServiceLifetime.Scoped
    )
    {
        serviceCollection.AddScoped(typeof(ICommandValidator<>), typeof(AdapterLoader<>));
        serviceCollection.RegisterGenericTypes(catalog, typeof(IValidator<>), validatorsLifetime);

        return serviceCollection;
    }
}
