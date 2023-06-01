using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.SmsSender;

public static class SmsSenderServiceCollectionExtensions
{
    public static IServiceCollection AddSmsSender(this IServiceCollection services, SmsApiConfiguration config)
    {
        services.AddSingleton(config);
        services.AddHttpClient<ISmsSender, SmsApiClient>(
            (sp, c) =>
            {
                var config = sp.GetRequiredService<SmsApiConfiguration>();

                SmsApiClient.ConfigureHttpClient(config, c);
            }
        );

        return services;
    }
}
