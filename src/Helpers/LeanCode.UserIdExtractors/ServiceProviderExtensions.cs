using LeanCode.DomainModels.Ids;
using LeanCode.UserIdExtractors.Extractors;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.UserIdExtractors;

public static class UserServiceProviderExtensions
{
    public static IServiceCollection AddUserIdExtractor<TUserId>(
        this IServiceCollection services,
        string userIdClaim = "sub"
    )
        where TUserId : notnull, IEquatable<TUserId>
    {
        var userIdType = typeof(TUserId);
        services.AddSingleton<IUserIdExtractor>(new StringUserIdExtractor(userIdClaim));

        if (userIdType == typeof(string))
        {
            services.AddSingleton<IUserIdExtractor<string>>(new GenericStringUserIdExtractor(userIdClaim));
        }
        else if (userIdType == typeof(Guid))
        {
            services.AddSingleton<IUserIdExtractor<Guid>>(new GuidUserIdExtractor(userIdClaim));
        }
        else if (
            !TryAddRawTypedUserIdExtractor(services, userIdType, userIdClaim)
            && !TryAddPrefixedTypedUserIdExtractor(services, userIdType, userIdClaim)
        )
        {
            throw new InvalidOperationException($"Type {userIdType} is not supported for user ID");
        }

        return services;
    }

    private static bool TryAddRawTypedUserIdExtractor(IServiceCollection services, Type userIdType, string userIdClaim)
    {
        var rawTypedId = TryGetImplementedGenericType(userIdType, typeof(IRawTypedId<,>));

        if (rawTypedId is not null)
        {
            var genericArguments = rawTypedId.GetGenericArguments();
            var backingType = genericArguments[0];
            var idType = genericArguments[1];

            var extractorType = typeof(RawTypedUserIdExtractor<,>).MakeGenericType(backingType, idType);
            var interfaceType = typeof(IUserIdExtractor<>).MakeGenericType(idType);
            var instance = Activator.CreateInstance(extractorType, userIdClaim)!;

            services.AddSingleton(interfaceType, instance);

            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool TryAddPrefixedTypedUserIdExtractor(
        IServiceCollection services,
        Type userIdType,
        string userIdClaim
    )
    {
        if (TryGetImplementedGenericType(userIdType, typeof(IPrefixedTypedId<>)) is not null)
        {
            var extractorType = typeof(PrefixedTypedUserIdExtractor<>).MakeGenericType(userIdType);
            var interfaceType = typeof(IUserIdExtractor<>).MakeGenericType(userIdType);
            var instance = Activator.CreateInstance(extractorType, userIdClaim)!;

            services.AddSingleton(interfaceType, instance);

            return true;
        }
        else
        {
            return false;
        }
    }

    private static Type? TryGetImplementedGenericType(Type type, Type idKind)
    {
        return type.IsValueType
            ? type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == idKind)
                .FirstOrDefault()
            : null;
    }
}
