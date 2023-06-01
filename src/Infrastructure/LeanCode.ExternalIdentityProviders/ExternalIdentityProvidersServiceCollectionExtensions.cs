using IdentityServer4.Validation;
using LeanCode.ExternalIdentityProviders.Apple;
using LeanCode.ExternalIdentityProviders.Facebook;
using LeanCode.ExternalIdentityProviders.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.ExternalIdentityProviders;

public static class ExternalIdentityProvidersServiceCollectionExtensions
{
    public static IServiceCollection AddExternalIdentityProviders<TUser>(
        this IServiceCollection services,
        Action<ExternalIdentityProviderBuilder<TUser>> config
    )
        where TUser : IdentityUser<Guid>
    {
        var builder = new ExternalIdentityProviderBuilder<TUser>(services);
        config(builder);
        builder.Validate();

        return services;
    }
}

public class ExternalIdentityProviderBuilder<TUser>
    where TUser : IdentityUser<Guid>
{
    private readonly IServiceCollection services;
    private bool anyProviderRegistered;

    public ExternalIdentityProviderBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public ExternalIdentityProviderBuilder<TUser> AddFacebook(FacebookConfiguration config)
    {
        anyProviderRegistered = true;

        services.AddSingleton(config);
        services.AddHttpClient<FacebookClient>(c => c.BaseAddress = new Uri(FacebookClient.ApiBase));
        services.TryAddTransient<FacebookExternalLogin<TUser>>();
        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IExtensionGrantValidator, FacebookGrantValidator<TUser>>()
        );

        return this;
    }

    public ExternalIdentityProviderBuilder<TUser> AddGoogle(GoogleAuthConfiguration config)
    {
        anyProviderRegistered = true;

        services.AddSingleton(config);
        services.TryAddTransient<GoogleAuthService>();
        services.TryAddTransient<GoogleExternalLogin<TUser>>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IExtensionGrantValidator, GoogleGrantValidator<TUser>>());

        return this;
    }

    public ExternalIdentityProviderBuilder<TUser> AddApple(AppleIdConfiguration config)
    {
        anyProviderRegistered = true;

        services.AddSingleton(config);
        services.AddHttpClient<AppleIdService>();
        services.TryAddTransient<AppleExternalLogin<TUser>>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IExtensionGrantValidator, AppleGrantValidator<TUser>>());

        return this;
    }

    public void Validate()
    {
        if (!anyProviderRegistered)
        {
            throw new InvalidOperationException(
                "No identity providers were registered, ensure you've added one via AddApple(...), AddGoogle(...), AddFacebook(...) methods"
            );
        }
    }
}
