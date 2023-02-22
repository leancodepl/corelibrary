using System.IdentityModel.Tokens.Jwt;
using Autofac;
using LeanCode.Components;
using LeanCode.IntegrationTestHelpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.IntegrationTests.App;

public class ApiModule : AppModule
{
    private readonly IConfiguration config;

    public ApiModule(IConfiguration config)
    {
        this.config = config;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA5404", Justification = "References don't go out of scope.")]
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<DbContextInitializer<TestDbContext>>();

        services.AddMemoryCache();

        services.AddIdentityServer()
            .AddInMemoryApiResources(AuthConfig.ApiResources)
            .AddInMemoryIdentityResources(AuthConfig.IdentityResources)
            .AddInMemoryApiScopes(AuthConfig.GetApiScopes())
            .AddInMemoryClients(AuthConfig.GetClients())
            .AddTestUsers(AuthConfig.GetUsers())
            .AddDeveloperSigningCredential();

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(
            options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.Authority = UrlHelper.Concat(config.GetValue<string>("InternalBase"), "auth");
                cfg.TokenValidationParameters.ValidateAudience = false;
                cfg.TokenValidationParameters.ValidateIssuer = false;
                cfg.RequireHttpsMetadata = false;

                cfg.TokenValidationParameters.RoleClaimType = "role";
            });

        services.AddDbContext<TestDbContext>(cfg =>
            cfg.UseSqlServer(config.GetValue<string>(ConfigurationOverrides.ConnectionStringKeyDefault)));
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AppRoles>().AsImplementedInterfaces();
    }
}
