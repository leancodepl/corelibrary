using LeanCode.Localization.StringLocalizers;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Localization;

public static class LocalizationServiceProviderExtensions
{
    public static IServiceCollection AddStringLocalizer(
        this IServiceCollection services,
        LocalizationConfiguration config
    )
    {
        services.AddSingleton<IStringLocalizer>(new ResourceManagerStringLocalizer(config));
        return services;
    }
}
