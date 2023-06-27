using LeanCode.Components;
using LeanCode.CQRS.AspNetCore;
using LeanCode.Startup.MicrosoftDI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.IntegrationTestHelpers.Tests.App;

public class Startup : LeanStartup
{
    protected override bool CloseAndFlushLogger { get; }

    public Startup(IConfiguration config)
        : base(config) { }

    public override void ConfigureServices(IServiceCollection services)
    {
        var connStr = new SqliteConnectionStringBuilder
        {
            DataSource = Guid.NewGuid().ToString("N"),
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        };
        services.AddDbContext<TestDbContext>(cfg => cfg.UseSqlite(connStr.ConnectionString));

        // The order here is important - we need to open the connection before migrations,
        // so that the DB is not dropped prematurely.
        services.AddHostedService<ConnectionKeeper>();
        services.AddHostedService<DbContextInitializer<TestDbContext>>();
        services.AddRouting();
        services.AddCQRS(TypesCatalog.Of<Command>(), TypesCatalog.Of<CommandCH>());
    }

    protected override void ConfigureApp(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseEndpoints(e => e.MapRemoteCqrs("/api", cqrs => { }));
    }
}
