using Autofac;
using ExampleApp.Core.Services.DataAccess;
using ExampleApp.Core.Services.DataAccess.Entities;
using LeanCode.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleApp.Core.Services;

public class CoreModule : AppModule
{
    private readonly string connectionString;

    public CoreModule(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<CoreDbContext>(
            opts => opts.UseSqlServer(connectionString, cfg => cfg.MigrationsAssembly("ExampleApp.Migrations"))
        );

        services.AddIdentity<AuthUser, AuthRole>().AddEntityFrameworkStores<CoreDbContext>().AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            options.User.AllowedUserNameCharacters =
                @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!#$%&'*+-/=?^_`{|}~.""(),:;<>@[\] ";
            options.Password.RequiredLength = 1;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
        });
    }

    protected override void Load(ContainerBuilder builder)
    {
        var self = typeof(CoreModule).Assembly;

        builder.Register(c => c.Resolve<CoreDbContext>()).AsImplementedInterfaces();
    }
}
