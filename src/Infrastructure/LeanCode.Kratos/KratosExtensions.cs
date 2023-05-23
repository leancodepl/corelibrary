using System.Diagnostics.CodeAnalysis;
using LeanCode.Kratos;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection;

public static class KratosExtensions
{
    public static IServiceCollection AddKratosClients(
        this IServiceCollection services,
        Action<KratosClientFactoryBuilder> configureClients
    )
    {
        configureClients(new(services));
        return services;
    }

    public static AuthenticationBuilder AddKratos<
        TOptions,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >(this AuthenticationBuilder builder, string authenticationScheme, Action<TOptions> configureOptions)
        where TOptions : KratosAuthenticationOptions, new()
        where THandler : KratosAuthenticationHandler<TOptions>
    {
        return builder.AddScheme<TOptions, THandler>(authenticationScheme, configureOptions);
    }

    public static AuthenticationBuilder AddKratos<
        TOptions,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >(this AuthenticationBuilder builder, Action<TOptions> configureOptions)
        where TOptions : KratosAuthenticationOptions, new()
        where THandler : KratosAuthenticationHandler<TOptions>
    {
        return builder.AddKratos<TOptions, THandler>(KratosDefaults.AuthenticationScheme, configureOptions);
    }

    public static AuthenticationBuilder AddKratos<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        Action<KratosAuthenticationOptions> configureOptions
    )
        where THandler : KratosAuthenticationHandler
    {
        return builder.AddScheme<KratosAuthenticationOptions, THandler>(authenticationScheme, configureOptions);
    }

    public static AuthenticationBuilder AddKratos<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >(this AuthenticationBuilder builder, Action<KratosAuthenticationOptions> configureOptions)
        where THandler : KratosAuthenticationHandler
    {
        return builder.AddKratos<THandler>(KratosDefaults.AuthenticationScheme, configureOptions);
    }

    public static AuthenticationBuilder AddKratos(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        Action<KratosAuthenticationOptions> configureOptions
    )
    {
        return builder.AddScheme<KratosAuthenticationOptions, KratosAuthenticationHandler>(
            authenticationScheme,
            configureOptions
        );
    }

    public static AuthenticationBuilder AddKratos(
        this AuthenticationBuilder builder,
        Action<KratosAuthenticationOptions> configureOptions
    )
    {
        return builder.AddKratos<KratosAuthenticationHandler>(KratosDefaults.AuthenticationScheme, configureOptions);
    }
}
