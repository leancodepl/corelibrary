using LeanCode.Components;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.AspNetCore.Serialization;
using LeanCode.CQRS.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.CQRS.AspNetCore;

public static class ServiceCollectionCQRSExtensions
{
    public static CQRSServicesBuilder AddCQRS(
        this IServiceCollection serviceCollection,
        TypesCatalog contractsCatalog,
        TypesCatalog handlersCatalog
    )
    {
        serviceCollection.AddSingleton<ISerializer>(_ => new Utf8JsonSerializer(Utf8JsonSerializer.DefaultOptions));

        var objectsSource = new CQRSObjectsRegistrationSource(contractsCatalog, handlersCatalog);

        serviceCollection.AddSingleton(objectsSource);
        serviceCollection.AddCQRSHandlers(objectsSource.Objects);

        return new CQRSServicesBuilder(serviceCollection);
    }
}

public class CQRSServicesBuilder
{
    private readonly IServiceCollection services;

    public CQRSServicesBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public CQRSServicesBuilder WithSerializer(ISerializer serializer)
    {
        services.Replace(new ServiceDescriptor(typeof(ISerializer), serializer));
        return this;
    }
}
