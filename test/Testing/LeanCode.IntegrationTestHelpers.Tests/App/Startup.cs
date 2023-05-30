using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.CQRS.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace LeanCode.IntegrationTestHelpers.Tests.App;

public class Startup : LeanStartup
{
    private static readonly TypesCatalog CQRSTypes = TypesCatalog.Of<Startup>();

    protected override IAppModule[] Modules { get; }
    protected override bool CloseAndFlushLogger { get; }

    public Startup(IConfiguration config)
        : base(config)
    {
        Modules = new IAppModule[] { new ApiModule(), };
    }

    protected override void ConfigureApp(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(e => e.MapRemoteCqrs("/api", cqrs => { }));
    }
}
