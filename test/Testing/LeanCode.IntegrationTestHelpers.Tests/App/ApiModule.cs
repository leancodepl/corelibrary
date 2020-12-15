using System;
using Autofac;
using LeanCode.Components;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.IntegrationTestHelpers.Tests.App
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
            var connStr = new SqliteConnectionStringBuilder
            {
                DataSource = Guid.NewGuid().ToString("N"),
                Mode = SqliteOpenMode.Memory,
                Cache = SqliteCacheMode.Shared,
            };
            services.AddDbContext<TestDbContext>(cfg => cfg.UseSqlite(connStr.ConnectionString));
        }

        protected override void Load(ContainerBuilder builder)
        { }
    }
}
