using IdentityServer4.Validation;
using LeanCode.ExternalIdentityProviders.Apple;
using LeanCode.ExternalIdentityProviders.Facebook;
using LeanCode.ExternalIdentityProviders.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.ExternalIdentityProviders.Tests;

public class ExternalIdentityProvidersModuleTests
{
    [Fact]
    public void Facebook_is_registered()
    {
        var services = Prepare(Providers.Facebook);

        Assert.NotNull(services.GetService<FacebookClient>());
        Assert.NotNull(services.GetService<FacebookExternalLogin<User>>());
        AssertExtensionGrantValidatorRegistered<FacebookGrantValidator<User>>(services);
    }

    [Fact]
    public void Apple_is_registered()
    {
        var services = Prepare(Providers.Apple);

        Assert.NotNull(services.GetService<AppleIdService>());
        Assert.NotNull(services.GetService<AppleExternalLogin<User>>());
        AssertExtensionGrantValidatorRegistered<AppleGrantValidator<User>>(services);
    }

    [Fact]
    public void Google_is_registered()
    {
        var services = Prepare(Providers.Google);

        Assert.NotNull(services.GetService<GoogleAuthService>());
        Assert.NotNull(services.GetService<GoogleExternalLogin<User>>());
        AssertExtensionGrantValidatorRegistered<GoogleGrantValidator<User>>(services);
    }

    [Fact]
    public void Registers_only_selected_providers()
    {
        var services = Prepare(Providers.Apple | Providers.Google);

        Assert.Null(services.GetService<FacebookExternalLogin<User>>());
        Assert.NotNull(services.GetService<AppleExternalLogin<User>>());
        Assert.NotNull(services.GetService<GoogleExternalLogin<User>>());

        AssertExtensionGrantValidatorRegistered<AppleGrantValidator<User>>(services);
        AssertExtensionGrantValidatorRegistered<GoogleGrantValidator<User>>(services);
        AssertExtensionGrantValidatorNotRegistered<FacebookGrantValidator<User>>(services);
    }

    [Fact]
    public void Throws_if_no_providers_are_selected()
    {
        Assert.Throws<ArgumentException>(() => Prepare(Providers.None));
    }

    private static void AssertExtensionGrantValidatorRegistered<TService>(IServiceProvider sp)
        where TService : IExtensionGrantValidator
    {
        var validators = sp.GetServices<IExtensionGrantValidator>();
        Assert.Contains(validators, v => v is TService);
    }

    private static void AssertExtensionGrantValidatorNotRegistered<TService>(IServiceProvider sp)
        where TService : IExtensionGrantValidator
    {
        var validators = sp.GetServices<IExtensionGrantValidator>();
        Assert.DoesNotContain(validators, v => v is TService);
    }

    private static IServiceProvider Prepare(Providers providers)
    {
        var services = new ServiceCollection();
        var module = new ExternalIdentityProvidersModule<User>(providers);

        services.AddMemoryCache();
        services.AddTransient<UserManager<User>>(s => UserManager.PrepareInMemory());

        services.AddSingleton<FacebookConfiguration>(new FacebookConfiguration(""));
        services.AddSingleton<GoogleAuthConfiguration>(new GoogleAuthConfiguration(""));
        services.AddSingleton<AppleIdConfiguration>(new AppleIdConfiguration(""));

        module.ConfigureServices(services);

        return services.BuildServiceProvider();
    }
}
