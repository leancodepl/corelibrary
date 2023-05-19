using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SendGrid;

namespace LeanCode.SendGrid;

public class SendGridModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddTransient(s => new SendGridClient(s.GetRequiredService<SendGridClientOptions>()));

        services.TryAddTransient<SendGridRazorClient>();
    }
}
