using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.Components;

public static class ServiceCollectionExtensions
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

        var implementationTypes = catalog
            .Assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(IsConcrete)
            .Select(type => new { Type = type, ImplementedServices = GetImplementedGenericTypes(type, genericType), })
            .Where(t => t.ImplementedServices.Any());

        foreach (var type in implementationTypes)
        {
            var services = type.ImplementedServices.Select(s => new ServiceDescriptor(s, type.Type, lifetime));
            serviceCollection.Add(services);
        }

        return serviceCollection;
    }

    private static IEnumerable<Type> GetImplementedGenericTypes(TypeInfo type, Type genericType)
    {
        return type.GetInterfaces()
            .Concat(GetBaseTypes(type))
            .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType);
    }

    private static IEnumerable<Type> GetBaseTypes(Type type)
    {
        var baseType = type.BaseType;
        while (baseType is not null)
        {
            yield return baseType;
            baseType = baseType.BaseType;
        }
    }

    private static bool IsOpenGeneric(this Type type)
    {
        return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
    }

    private static bool IsConcrete(Type type)
    {
        return type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false };
    }
}
