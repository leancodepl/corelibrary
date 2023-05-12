using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.CQRS.AspNetCore.Registration;

public static class ServiceCollectionRegistrationExtensions
{
    public static IServiceCollection RegisterGenericTypes(
        this IServiceCollection serviceCollection,
        TypesCatalog catalog,
        Type genericType,
        ServiceLifetime lifetime
        )
    {
        if (!IsOpenGeneric(genericType))
        {
            throw new InvalidOperationException($"Type {genericType} is not an open generic type");
        }

        var implementationTypes = catalog.Assemblies.SelectMany(a => a.DefinedTypes)
            .Where(IsConcrete)
            .Select(type => new
            {
                Type = type,
                ImplementedInterfaces = GetImplementedGenericTypes(type, genericType),
            })
            .Where(t => t.ImplementedInterfaces.Any());

        foreach (var type in implementationTypes)
        {
            var services = type.ImplementedInterfaces.Select(s => new ServiceDescriptor(s, type.Type, lifetime));
            serviceCollection.Add(services);
        }

        return serviceCollection;
    }

    private static IEnumerable<Type> GetImplementedGenericTypes(Type type, Type genericType)
    {
        return type.GetInterfaces()
            .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType);
    }

    private static bool IsOpenGeneric(this Type type)
    {
        return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
    }

    private static bool IsConcrete(Type type)
    {
        return type is
        {
            IsClass: true,
            IsAbstract: false,
            ContainsGenericParameters: false
        };
    }
}
