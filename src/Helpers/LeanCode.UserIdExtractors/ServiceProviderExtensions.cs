using System.Reflection;
using LeanCode.DomainModels.Ids;
using LeanCode.UserIdExtractors.Extractors;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.UserIdExtractors;

public static class UserServiceProviderExtensions
{
    public static IServiceCollection AddUserIdExtractors(
        this IServiceCollection services,
        Assembly[] idAssemblies,
        string userIdClaim = "sub"
    )
    {
        services.AddBasicUserIdExtractors(userIdClaim);
        services.AddPrefixedTypedUserIdExtractors(idAssemblies, userIdClaim);
        services.AddRawTypedUserIdExtractors(idAssemblies, userIdClaim);

        return services;
    }

    private static void AddBasicUserIdExtractors(this IServiceCollection services, string userIdClaim)
    {
        var stringInstance = Activator.CreateInstance(typeof(StringUserIdExtractor), userIdClaim)!;
        var genericStringInstance = Activator.CreateInstance(typeof(GenericStringUserIdExtractor), userIdClaim)!;
        var guidInstance = Activator.CreateInstance(typeof(GuidUserIdExtractor), userIdClaim)!;

        services.AddSingleton(typeof(IUserIdExtractor), stringInstance);
        services.AddSingleton(typeof(IUserIdExtractor<string>), genericStringInstance);
        services.AddSingleton(typeof(IUserIdExtractor<Guid>), guidInstance);
    }

    private static void AddPrefixedTypedUserIdExtractors(
        this IServiceCollection services,
        Assembly[] idAssemblies,
        string userIdClaim
    )
    {
        var prefixedTypedIdTypes = GetIdTypes(typeof(IPrefixedTypedId<>), idAssemblies);

        foreach (var type in prefixedTypedIdTypes)
        {
            var extractorType = typeof(PrefixedTypedUserIdExtractor<>).MakeGenericType(type);
            var interfaceType = typeof(IUserIdExtractor<>).MakeGenericType(type);
            var instance = Activator.CreateInstance(extractorType, userIdClaim)!;

            services.AddSingleton(interfaceType, instance);
        }
    }

    private static void AddRawTypedUserIdExtractors(
        this IServiceCollection services,
        Assembly[] idAssemblies,
        string userIdClaim
    )
    {
        var rawTypedIdTypes = GetIdTypes(typeof(IRawTypedId<,>), idAssemblies);

        foreach (var type in rawTypedIdTypes)
        {
            var rawTypedIdInterface = type.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRawTypedId<,>));

            var genericArguments = rawTypedIdInterface.GetGenericArguments();
            var backingType = genericArguments[0];
            var idType = genericArguments[1];

            var extractorType = typeof(RawTypedUserIdExtractor<,>).MakeGenericType(backingType, idType);
            var interfaceType = typeof(IUserIdExtractor<>).MakeGenericType(idType);
            var instance = Activator.CreateInstance(extractorType, userIdClaim)!;

            services.AddSingleton(interfaceType, instance);
        }
    }

    private static IEnumerable<Type> GetIdTypes(Type idKind, Assembly[] idAssemblies)
    {
        return idAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(
                t =>
                    t.IsValueType
                    && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == idKind)
            );
    }
}
