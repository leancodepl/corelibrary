using LeanCode.Components;
using LeanCode.CQRS.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.CQRS.AspNetCore.Registration;

public static class ServiceCollectionRegistrationExtensions
{
    public static IServiceCollection AddCQRSHandlers(
        this IServiceCollection serviceCollection,
        IEnumerable<CQRSObjectMetadata> cqrsObjects
    )
    {
        foreach (var obj in cqrsObjects)
        {
            serviceCollection.Add(
                new ServiceDescriptor(MakeHandlerInterfaceType(obj), obj.HandlerType, ServiceLifetime.Scoped)
            );
        }

        return serviceCollection;

        Type MakeHandlerInterfaceType(CQRSObjectMetadata obj)
        {
            return obj.ObjectKind switch
            {
                CQRSObjectKind.Command => typeof(ICommandHandler<,>).MakeGenericType(obj.ContextType, obj.ObjectType),
                CQRSObjectKind.Query
                    => typeof(IQueryHandler<,,>).MakeGenericType(obj.ContextType, obj.ObjectType, obj.ResultType),
                CQRSObjectKind.Operation
                    => typeof(IOperationHandler<,,>).MakeGenericType(obj.ContextType, obj.ObjectType, obj.ResultType),
                _ => throw new InvalidOperationException("Unexpected object kind"),
            };
        }
    }

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

        var implementationTypes = catalog.Assemblies
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

    private static IEnumerable<Type> GetImplementedGenericTypes(Type type, Type genericType)
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
