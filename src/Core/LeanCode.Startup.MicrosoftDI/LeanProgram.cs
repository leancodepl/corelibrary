using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LeanCode.Startup.MicrosoftDI;

public static class LeanProgram
{
    public const string SystemLoggersEntryName = "Serilog:SystemLoggers";

    public static IHostBuilder BuildMinimalHost<TStartup>()
        where TStartup : class
    {
        return new HostBuilder()
            .ConfigureAppConfiguration(
                (hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                }
            )
            .ConfigureWebHost(builder =>
            {
                builder
                    .UseKestrel(
                        (builderContext, options) =>
                        {
                            options.Configure(builderContext.Configuration.GetSection("Kestrel"), reloadOnChange: true);
                        }
                    )
                    .UseStartup<TStartup>();
            });
    }
}
