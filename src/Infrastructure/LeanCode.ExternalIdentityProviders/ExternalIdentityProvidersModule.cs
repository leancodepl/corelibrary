using LeanCode.Components;
using LeanCode.ExternalIdentityProviders.Apple;
using LeanCode.ExternalIdentityProviders.Facebook;
using LeanCode.ExternalIdentityProviders.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.ExternalIdentityProviders;

[Flags]
public enum Providers
{
    Facebook = 0b001,
    Apple = 0b010,
    Google = 0b100,

    None = 0b000,
    All = 0b111,
}

public class ExternalIdentityProvidersModule<TUser> : AppModule
    where TUser : IdentityUser<Guid>
{
    private readonly Providers configuration;

    public ExternalIdentityProvidersModule(Providers configuration)
    {
        if ((configuration & Providers.All) == Providers.None)
        {
            throw new ArgumentException("At least one identity provider is required.", nameof(configuration));
        }

        this.configuration = configuration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        if (IsEnabled(Providers.Facebook))
        {
            services.AddHttpClient<FacebookClient>(c => c.BaseAddress = new Uri(FacebookClient.ApiBase));

            services.TryAddTransient<FacebookExternalLogin<TUser>>();
            services.TryRegisterWithImplementedInterfaces<FacebookGrantValidator<TUser>>();
        }

        if (IsEnabled(Providers.Apple))
        {
            services.AddHttpClient<AppleIdService>();

            services.TryAddTransient<AppleExternalLogin<TUser>>();
            services.TryRegisterWithImplementedInterfaces<AppleGrantValidator<TUser>>();
        }

        if (IsEnabled(Providers.Google))
        {
            services.TryAddTransient<GoogleAuthService>();
            services.TryAddTransient<GoogleExternalLogin<TUser>>();
            services.TryRegisterWithImplementedInterfaces<GoogleGrantValidator<TUser>>();
        }

        services.TryAddTransient(typeof(ExternalLoginExceptionHandler<>));
    }

    private bool IsEnabled(Providers provider) => configuration.HasFlag(provider);
}
