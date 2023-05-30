using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SendGrid;

namespace LeanCode.SendGrid;

public static class SendGridServiceCollectionExtensions
{
    public static IServiceCollection AddSendGridClient(this IServiceCollection services, SendGridClientOptions config)
    {
        services.AddSingleton(config);
        services.TryAddTransient(s => new SendGridClient(s.GetRequiredService<SendGridClientOptions>()));
        services.TryAddTransient<SendGridRazorClient>();
        return services;
    }
}
