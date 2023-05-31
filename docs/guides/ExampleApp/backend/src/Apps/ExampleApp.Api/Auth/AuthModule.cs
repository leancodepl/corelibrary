using System.IdentityModel.Tokens.Jwt;
using Autofac;
using IdentityServer4.Services;
using LeanCode.Components;
using ExampleApp.Core.Services.DataAccess.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AuthConsts = ExampleApp.Core.Contracts.Auth;

namespace ExampleApp.Api.Auth;

public class AuthModule : AppModule
{
    private readonly IWebHostEnvironment hostEnv;
    private readonly IConfiguration config;

    public AuthModule(IWebHostEnvironment hostEnv, IConfiguration config)
    {
        this.hostEnv = hostEnv;
        this.config = config;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Config.SqlServer.ConnectionString(config);

        var isConfig = services.AddIdentityServer()
            .AddInMemoryApiResources(ClientsConfiguration.GetApiResources())
            .AddInMemoryIdentityResources(ClientsConfiguration.GetIdentityResources())
            .AddInMemoryClients(ClientsConfiguration.GetClients())
            .AddInMemoryApiScopes(ClientsConfiguration.GetApiScopes())
            .AddOperationalStore(options =>
            {
                options.DefaultSchema = "auth";
                options.ConfigureDbContext = b => b
                    .UseSqlServer(
                        connectionString,
                        sql => sql.MigrationsAssembly("ExampleApp.Migrations"));
            })
            .AddAspNetIdentity<AuthUser>();

        if (hostEnv.IsDevelopment())
        {
            isConfig = isConfig.AddDeveloperSigningCredential();
        }

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
            .AddJwtBearer(cfg =>
            {
                cfg.Authority = Config.Services.Auth.Address(config);
                cfg.TokenValidationParameters.ValidateAudience = true;
                cfg.TokenValidationParameters.ValidateIssuer = true;
                cfg.TokenValidationParameters.ValidAudience = AuthConsts.Scopes.InternalApi;
                cfg.TokenValidationParameters.ValidIssuer = Config.Services.Auth.ExternalAddress(config);
                cfg.RequireHttpsMetadata = false;

                cfg.TokenValidationParameters.RoleClaimType = AuthConsts.KnownClaims.Role;
                cfg.TokenValidationParameters.NameClaimType = AuthConsts.KnownClaims.UserId;
            });
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(ctx =>
        {
            var corsConfig = Config.Services.AllowedOrigins(config);

            var logger = ctx.Resolve<ILogger<DefaultCorsPolicyService>>();
            return new DefaultCorsPolicyService(logger)
            {
                AllowedOrigins = corsConfig,
            };
        }).As<ICorsPolicyService>().SingleInstance();
    }
}
