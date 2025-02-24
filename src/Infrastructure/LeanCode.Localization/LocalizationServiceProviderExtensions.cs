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
        services.AddSingleton<IStringLocalizer>(sp => new ResourceManagerStringLocalizer(
            sp.GetRequiredService<Serilog.ILogger>(),
            config
        ));
        return services;
    }
}
