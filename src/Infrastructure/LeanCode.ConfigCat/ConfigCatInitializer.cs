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

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031:DoNotCatchGeneralExceptionTypes",
        Justification = "We don't want any exceptions to be propagated."
    )]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await configCatClient.GetAllKeysAsync(stoppingToken);
        }
        catch { }
    }
}
