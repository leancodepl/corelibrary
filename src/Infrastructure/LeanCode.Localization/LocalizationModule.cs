using LeanCode.Components;
using LeanCode.Localization.StringLocalizers;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Localization;

public class LocalizationModule : AppModule
{
    private readonly LocalizationConfiguration config;

    public LocalizationModule(LocalizationConfiguration config)
    {
        this.config = config;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IStringLocalizer>(new ResourceManagerStringLocalizer(config));
    }
}
