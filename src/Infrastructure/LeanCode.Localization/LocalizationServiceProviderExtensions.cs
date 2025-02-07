using LeanCode.Localization.StringLocalizers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeanCode.Localization;

public static class LocalizationServiceProviderExtensions
{
    public static IServiceCollection AddStringLocalizer(
        this IServiceCollection services,
        LocalizationConfiguration config
    )
    {
        services.AddSingleton<IStringLocalizer>(sp => new ResourceManagerStringLocalizer(
            sp.GetRequiredService<ILogger<ResourceManagerStringLocalizer>>(),
            config
        ));
        return services;
    }
}
