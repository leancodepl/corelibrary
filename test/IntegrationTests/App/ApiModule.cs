using Autofac;
using LeanCode.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.IntegrationTests.App
{
    public class ApiModule : AppModule
    {
        private readonly IConfiguration config;

        public ApiModule(IConfiguration config)
        {
            this.config = config;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TestDbContext>(cfg =>
                cfg.UseSqlServer(config.GetConnectionString("Database")));
        }

        protected override void Load(ContainerBuilder builder)
        { }
    }
}
