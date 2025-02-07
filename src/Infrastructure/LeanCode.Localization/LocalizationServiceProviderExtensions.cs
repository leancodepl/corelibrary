using LeanCode.Localization.StringLocalizers;
using LeanCode.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Localization;

public static class LocalizationServiceProviderExtensions
{
    public static IServiceCollection AddStringLocalizer(
        this IServiceCollection services,
        LocalizationConfiguration config
    )
    {
        return services.AddSingleton<IStringLocalizer>(sp => new ResourceManagerStringLocalizer(
            config,
            sp.GetRequiredService<ILogger<ResourceManagerStringLocalizer>>()
        ));
    }
}
