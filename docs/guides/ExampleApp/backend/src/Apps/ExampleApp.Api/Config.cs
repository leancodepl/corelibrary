using Autofac;
using LeanCode;
using LeanCode.IdentityServer.KeyVault;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace ExampleApp.Api;

public static class Config
{
    public static class App
    {
        public static string PublicDomain(IConfiguration cfg) => cfg.GetString("Domains:Public");

        public static string InternalApiDomain(IConfiguration cfg) => cfg.GetString("Domains:ApiInternal");

        public static string InternalApiBase(IConfiguration cfg) => $"http://{InternalApiDomain(cfg)}";

        public static string PublicComponent(IConfiguration cfg, string component) =>
            $"https://{component}.{PublicDomain(cfg)}";
    }

    public static class KeyVault
    {
        private const string KeyAddress = "keys/token-signing";

        public static string VaultUrl(IConfiguration cfg) => cfg.GetString("KeyVault:VaultUrl") ?? string.Empty;

        public static string KeyId(IConfiguration cfg) => UrlHelper.Concat(VaultUrl(cfg), KeyAddress);
    }

    public static class SqlServer
    {
        public static string ConnectionString(IConfiguration cfg) => cfg.GetString("SqlServer:ConnectionString");
    }

    public static class BlobStorage
    {
        public static string ConnectionString(IConfiguration cfg) => cfg.GetString("BlobStorage:ConnectionString");
    }

    public static class MassTransit
    {
        public static class AzureServiceBus
        {
            public static string Endpoint(IConfiguration cfg) => cfg.GetString("MassTransit:AzureServiceBus:Endpoint");
        }
    }

    public static class Services
    {
        public static string[] AllowedOrigins(IConfiguration cfg) =>
            ExternalApps(cfg).Concat(Array.Empty<string>()).ToArray();

        public static string[] ExternalApps(IConfiguration cfg) =>
            cfg?.GetSection("CORS:External").Get<string[]>() ?? Array.Empty<string>();

        public static class Auth
        {
            public static string Address(IConfiguration cfg) => UrlHelper.Concat(App.InternalApiBase(cfg), "auth");

            public static string ExternalAddress(IConfiguration cfg) =>
                UrlHelper.Concat(App.PublicComponent(cfg, "api"), "auth");
        }
    }

    public static class Logging
    {
        public static bool EnableDetailedInternalLogs(IConfiguration cfg) =>
            cfg.GetBool("Logging:EnableDetailedInternalLogs");

        public static LogEventLevel MinimumLevel(IConfiguration cfg) =>
            cfg.GetValue("Logging:MinimumLevel", LogEventLevel.Verbose);
    }

    public static class Telemetry
    {
        public static string? OtlpEndpoint(IConfiguration cfg) => cfg.GetString("Telemetry:OtlpEndpoint");
    }

    private static string GetString(this IConfiguration configuration, string key)
    {
        return configuration.GetValue<string>(key)!;
    }

    private static bool GetBool(this IConfiguration configuration, string key)
    {
        return configuration.GetValue<bool>(key)!;
    }

    private static string GetService(this IConfiguration configuration, string rest)
    {
        return configuration.GetValue<string>($"Services:{rest}")!;
    }

    private static string GetSecret(this IConfiguration configuration, string api)
    {
        return configuration.GetValue<string>($"Secrets:{api}")!;
    }

    public static void RegisterConfig<TConfig>(this ContainerBuilder builder, TConfig config)
        where TConfig : class
    {
        builder.RegisterInstance(config).AsSelf().SingleInstance();
    }

    public static void RegisterMappedConfiguration(
        ContainerBuilder builder,
        IConfiguration config,
        IWebHostEnvironment hostEnv
    ) { }
}
