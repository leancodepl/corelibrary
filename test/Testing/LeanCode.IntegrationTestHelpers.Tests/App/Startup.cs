using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.RemoteHttp.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace LeanCode.IntegrationTestHelpers.Tests.App
{
    public class Startup : LeanStartup
    {
        private static readonly TypesCatalog CQRSTypes = TypesCatalog.Of<Startup>();

        protected override IAppModule[] Modules { get; }

        public Startup(IConfiguration config)
            : base(config)
        {
            Modules = new IAppModule[]
            {
                new ApiModule(),
                new CQRSModule()
                    .WithCustomPipelines<Context>(CQRSTypes, c => c, q => q),
            };
        }

        protected override void ConfigureApp(IApplicationBuilder app)
        {
            app.Map("/api", x => x.UseRemoteCQRS(CQRSTypes, c => new Context()));
        }
    }
}
