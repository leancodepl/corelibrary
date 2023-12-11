using LeanCode.DomainModels.Ids;
using LeanCode.UserIdExtractors.Extractors;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.UserIdExtractors;

public static class UserServiceProviderExtensions
{
    private const string DefaultUserIdClaim = "sub";

    public static IServiceCollection AddStringUserIdExtractor(
        this IServiceCollection services,
        string userIdClaim = DefaultUserIdClaim
    )
    {
        services.AddSingleton<IUserIdExtractor>(new StringUserIdExtractor(userIdClaim));
        services.AddSingleton<IUserIdExtractor<string>>(new GenericStringUserIdExtractor(userIdClaim));

        return services;
    }

    public static IServiceCollection AddGuidUserIdExtractor(
        this IServiceCollection services,
        string userIdClaim = DefaultUserIdClaim
    )
    {
        services.AddSingleton<IUserIdExtractor>(new StringUserIdExtractor(userIdClaim));
        services.AddSingleton<IUserIdExtractor<Guid>>(new GuidUserIdExtractor(userIdClaim));

        return services;
    }

    public static IServiceCollection AddRawTypedUserIdExtractor<TBacking, TUserId>(
        this IServiceCollection services,
        string userIdClaim = DefaultUserIdClaim
    )
        where TBacking : struct
        where TUserId : struct, IRawTypedId<TBacking, TUserId>
    {
        services.AddSingleton<IUserIdExtractor>(new StringUserIdExtractor(userIdClaim));
        services.AddSingleton<IUserIdExtractor<TUserId>>(new RawTypedUserIdExtractor<TBacking, TUserId>(userIdClaim));

        return services;
    }

    public static IServiceCollection AddPrefixedUserIdExtractor<TUserId>(
        this IServiceCollection services,
        string userIdClaim = DefaultUserIdClaim
    )
        where TUserId : struct, IPrefixedTypedId<TUserId>
    {
        services.AddSingleton<IUserIdExtractor>(new StringUserIdExtractor(userIdClaim));
        services.AddSingleton<IUserIdExtractor<TUserId>>(new PrefixedTypedUserIdExtractor<TUserId>(userIdClaim));

        return services;
    }
}
