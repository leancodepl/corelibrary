using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeanCode.EFMigrator
{
    public abstract class BaseFactory<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : DbContext
    {
        protected static string AssemblyName;

        static BaseFactory()
        {
            AssemblyName = Assembly.GetEntryAssembly().GetName().Name;
        }

        public TContext CreateDbContext(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(cfg => cfg.AddConsole());
            var provider = services.BuildServiceProvider();

            var opts = new DbContextOptionsBuilder<TContext>()
                .UseLoggerFactory(provider.GetRequiredService<ILoggerFactory>())
                .UseSqlServer(
                    MigrationsConfig.GetConnectionString(),
                    cfg => cfg.MigrationsAssembly(AssemblyName))
                .Options;

            return (TContext)Activator.CreateInstance(typeof(TContext), opts);
        }
    }
}
