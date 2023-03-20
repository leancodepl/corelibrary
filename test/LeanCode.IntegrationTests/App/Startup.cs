using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.RemoteHttp.Server;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.CQRS.Validation.Fluent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace LeanCode.IntegrationTests.App;

public class Startup : LeanStartup
{
    private static readonly TypesCatalog CQRSTypes = TypesCatalog.Of<ApiModule>();

    protected override IAppModule[] Modules { get; }

    public Startup(IConfiguration config)
        : base(config)
    {
        Modules = new IAppModule[]
        {
            new CQRSModule().WithCustomPipelines<AppContext>(
                CQRSTypes,
                c => c.Secure().Validate(),
                q => q.Secure(),
                o => o.Secure()
            ),
            new FluentValidationModule(CQRSTypes),
            new ApiModule(config),
        };

        CloseAndFlushLogger = false;
    }

    protected override void ConfigureApp(IApplicationBuilder app)
    {
        app.Map("/auth", inner => inner.UseIdentityServer())
            .Map("/api", cfg => cfg.UseAuthentication().UseRemoteCQRS(CQRSTypes, AppContext.FromHttp));
    }
}
