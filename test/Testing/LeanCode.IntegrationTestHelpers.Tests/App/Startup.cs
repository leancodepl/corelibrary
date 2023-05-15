using LeanCode.Components;
using LeanCode.Components.Startup;
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
        Modules = new IAppModule[]
        {
            new ApiModule(),
            // TODO: restore
            // new CQRSModule().WithCustomPipelines<Context>(CQRSTypes, c => c, q => q, o => o),
        };
    }

    protected override void ConfigureApp(IApplicationBuilder app)
    {
        // TODO: restore
        // app.Map("/api", x => x.UseRemoteCQRS(CQRSTypes, c => new Context()));
    }
}
