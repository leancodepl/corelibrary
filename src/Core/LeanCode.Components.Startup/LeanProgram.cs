using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LeanCode.Components.Startup
{
    public static class LeanProgram
    {
        public const string SystemLoggersEntryName = "Serilog:SystemLoggers";

        public static IWebHostBuilder BuildMinimalWebHost<TStartup>()
            where TStartup : class
        {
            return new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .UseStartup<TStartup>();
        }
    }
}
