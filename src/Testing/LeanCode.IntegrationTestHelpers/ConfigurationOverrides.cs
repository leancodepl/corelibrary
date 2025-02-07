using LeanCode.Logging.AspNetCore;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace LeanCode.IntegrationTestHelpers;

public class ConfigurationOverrides(Dictionary<string, string?> customValues) : IConfigurationSource
{
    public static ConfigurationOverrides LoggingOverrides(
        LogEventLevel minimumLevel = LogEventLevel.Information,
        bool enableInternalLogs = true
    )
    {
        return new ConfigurationOverrides(
            new Dictionary<string, string?>
            {
                [IHostBuilderExtensions.MinimumLogLevelKey] = minimumLevel.ToString(),
                [IHostBuilderExtensions.EnableDetailedInternalLogsKey] = enableInternalLogs.ToString(),
            }
        );
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(customValues);

    private sealed class Provider : ConfigurationProvider
    {
        public Provider(Dictionary<string, string?> data)
        {
            Data = data;
        }
    }
}
