using System.Text.Json;
using ConfigCat.Client;
using LeanCode.ConfigCat;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "?",
    "IDE0130:NamespaceDoesNotMatchFolderStructure",
    Justification = "Extensions on IServiceCollection by convention live in Microsoft.Extensions.DependencyInjection namespace.",
    Scope = "namespace",
    Target = "~N:Microsoft.Extensions.DependencyInjection"
)]

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigCatExtensions
{
    public static IServiceCollection AddConfigCat(
        this IServiceCollection services,
        string? sdkKey,
        string? flagOverridesFilePath,
        bool autoReload = true,
        OverrideBehaviour overrideBehaviour = OverrideBehaviour.LocalOnly,
        bool initializeOnStartup = false
    )
    {
        return services.AddConfigCat(
            sdkKey,
            flagOverridesFilePath is null
                ? null
                : FlagOverrides.LocalFile(flagOverridesFilePath, autoReload, overrideBehaviour),
            initializeOnStartup
        );
    }

    public static IServiceCollection AddConfigCat(
        this IServiceCollection services,
        string? sdkKey,
        IDictionary<string, object>? flagOverrides,
        bool watchChanges = false,
        OverrideBehaviour overrideBehaviour = OverrideBehaviour.LocalOnly,
        bool initializeOnStartup = false
    )
    {
        return services.AddConfigCat(
            sdkKey,
            flagOverrides is null
                ? null
                : FlagOverrides.LocalDictionary(flagOverrides, watchChanges, overrideBehaviour),
            initializeOnStartup
        );
    }

    public static IServiceCollection AddConfigCat(
        this IServiceCollection services,
        string? sdkKey,
        FlagOverrides? flagOverrides = null,
        bool initializeOnStartup = false
    )
    {
        if (sdkKey is not { Length: > 0 } && flagOverrides is null)
        {
            throw new InvalidOperationException("At least one feature flags configuration method is needed.");
        }

        services.TryAddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ConfigCatClient>>();

            return ConfigCatClient.Get(
                sdkKey ?? "localhost",
                options =>
                {
                    options.Logger = new ConfigCatToMSLoggerAdapter(logger);
                    options.FlagOverrides = flagOverrides;
                    options.Offline =
                        flagOverrides is not null && flagOverrides.OverrideBehaviour == OverrideBehaviour.LocalOnly;
                }
            );
        });

        return services.TryAddConfigCatInitializer(initializeOnStartup);
    }

    public static IServiceCollection AddConfigCat(this IServiceCollection services, bool initializeOnStartup = false)
    {
        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ConfigCatOptions>>().Value;

            var flagOverrides = options switch
            {
                { FlagOverridesJsonObject: { } json }
                    when JsonSerializer.Deserialize<IDictionary<string, object>>(json) is { } dictionary
                    => FlagOverrides.LocalDictionary(dictionary, watchChanges: false, OverrideBehaviour.LocalOnly),
                { FlagOverridesFilePath: { Length: > 0 } filePath }
                    => FlagOverrides.LocalFile(filePath, autoReload: true, OverrideBehaviour.LocalOnly),
                _ => null,
            };

            if (options.SdkKey is not { Length: > 0 } && flagOverrides is null)
            {
                throw new InvalidOperationException("At least one feature flags configuration method is needed.");
            }

            var logger = sp.GetRequiredService<ILogger<ConfigCatClient>>();

            return ConfigCatClient.Get(
                options.SdkKey ?? "localhost",
                options =>
                {
                    options.Logger = new ConfigCatToMSLoggerAdapter(logger);
                    options.FlagOverrides = flagOverrides;
                    options.Offline = flagOverrides is not null;
                }
            );
        });

        return services.TryAddConfigCatInitializer(initializeOnStartup);
    }

    private static IServiceCollection TryAddConfigCatInitializer(this IServiceCollection services, bool add)
    {
        return add ? services.AddHostedService<ConfigCatInitializer>() : services;
    }
}
