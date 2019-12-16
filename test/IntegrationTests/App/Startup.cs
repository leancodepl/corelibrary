using LeanCode.AsyncInitializer;
using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.Firebase.FCM;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace LeanCode.IntegrationTests.App
{
    public class Startup : LeanStartup
    {
        protected override IAppModule[] Modules { get; }

        public Startup(IConfiguration config)
            : base(config)
        {
            Modules = new IAppModule[]
            {
                new AsyncInitializerModule(),
                new FCMModule<TestDbContext>(),
                new ApiModule(config),
            };
        }

        protected override void ConfigureApp(IApplicationBuilder app)
        {
        }
    }
}
