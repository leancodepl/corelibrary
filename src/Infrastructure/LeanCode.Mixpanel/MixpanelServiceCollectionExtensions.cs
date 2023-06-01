using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Mixpanel;

public static class MixpanelServiceCollectionExtensions
{
    public static IServiceCollection AddMixpanel(this IServiceCollection services, MixpanelConfiguration config)
    {
        services.AddSingleton(config);
        services.AddHttpClient<MixpanelAnalytics>(c => c.BaseAddress = new Uri("https://api.mixpanel.com"));
        return services;
    }
}
