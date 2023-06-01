using FluentValidation;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Validation.Fluent.Scoped;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddFluentValidation(
        this IServiceCollection serviceCollection,
        TypesCatalog catalog
    )
    {
        serviceCollection.AddScoped(typeof(ICommandValidator<>), typeof(AdapterLoader<>));
        serviceCollection.RegisterGenericTypes(catalog, typeof(IValidator<>), ServiceLifetime.Scoped);

        return serviceCollection;
    }
}
