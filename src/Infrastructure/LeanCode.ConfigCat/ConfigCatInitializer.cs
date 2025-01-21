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
        Justification = "We want to prevent exceptions from propagating to ensure the application does not fail."
    )]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await configCatClient.GetAllKeysAsync(stoppingToken);
        }
        catch
        {
            // We want to prevent exceptions from propagating to ensure the application does not fail.
        }
    }
}
