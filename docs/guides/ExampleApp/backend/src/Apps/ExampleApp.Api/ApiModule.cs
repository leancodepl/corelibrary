using Autofac;
using LeanCode.AzureIdentity;
using LeanCode.Components;
using LeanCode.IdentityServer.KeyVault;
using LeanCode.OpenTelemetry;
using ExampleApp.Core.Services.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ExampleApp.Api;

internal class ApiModule : AppModule
{
    internal const string ApiCorsPolicy = "Api";

    private readonly IConfiguration config;
    private readonly IWebHostEnvironment hostEnv;

    public ApiModule(IConfiguration config, IWebHostEnvironment hostEnv)
    {
        this.config = config;
        this.hostEnv = hostEnv;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(ConfigureCORS);
        services.AddRouting();
        services.AddHealthChecks()
            .AddDbContextCheck<CoreDbContext>();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        var otlp = Config.Telemetry.OtlpEndpoint(config);

        if (!string.IsNullOrWhiteSpace(otlp))
        {
            services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation(opts => opts.Filter = ctx =>
                    {
                        return !ctx.Request.Path.StartsWithSegments("/live");
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(opts => opts.SetDbStatementForText = true)
                    .AddLeanCodeTelemetry()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ExampleApp.Api"))
                    .AddOtlpExporter(cfg => cfg.Endpoint = new(otlp));
            });
        }

        services.AddAzureClients(cfg =>
        {
            cfg.AddBlobServiceClient(Config.BlobStorage.ConnectionString(config));

            if (!hostEnv.IsDevelopment())
            {
                cfg.AddKeyClient(new(Config.KeyVault.VaultUrl(config)));
                cfg.AddIdentityServerTokenSigningKey(new(Config.KeyVault.KeyId(config)));
                cfg.UseCredential(DefaultLeanCodeCredential.Create(config));
            }
        });
    }

    protected override void Load(ContainerBuilder builder)
    {
        Config.RegisterMappedConfiguration(builder, config, hostEnv);

        builder.RegisterType<AppRoles>()
            .AsImplementedInterfaces();
    }

    private void ConfigureCORS(CorsOptions opts)
    {
        opts.AddPolicy(ApiCorsPolicy, cfg =>
        {
            cfg
                .WithOrigins(Config.Services.AllowedOrigins(config))
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromMinutes(60));
        });
    }
}
