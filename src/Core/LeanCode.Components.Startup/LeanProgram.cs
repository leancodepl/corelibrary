using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LeanCode.Components.Startup
{
    public static class LeanProgram
    {
        public const string SystemLoggersEntryName = "Serilog:SystemLoggers";

        public static IHostBuilder BuildMinimalHost<TStartup>()
            where TStartup : class
        {
            return new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .UseKestrel((builderContext, options) =>
                        {
                            options.Configure(builderContext.Configuration.GetSection("Kestrel"), reloadOnChange: true);
                        })
                        .UseStartup<TStartup>();
                });
        }
    }
}
