using ConfigCat.Client;
using Microsoft.Extensions.Hosting;

namespace LeanCode.ConfigCat;

public sealed class ConfigCatInitializer : BackgroundService
{
    private readonly IConfigCatClient configCatClient;

    public ConfigCatInitializer(IConfigCatClient configCatClient)
    {
        this.configCatClient = configCatClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await configCatClient.GetAllKeysAsync(stoppingToken);
    }
}
