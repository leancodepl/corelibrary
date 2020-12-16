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
        }

        protected override void Load(ContainerBuilder builder)
        { }
    }
}
