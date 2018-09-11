using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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
            var factory = new LoggerFactory()
                .AddConsole();
            var opts = new DbContextOptionsBuilder<TContext>()
                .UseLoggerFactory(factory)
                .UseSqlServer(
                    MigrationsConfig.GetConnectionString(),
                    cfg => cfg.MigrationsAssembly(AssemblyName))
                .Options;

            return (TContext)Activator.CreateInstance(typeof(TContext), opts);
        }
    }
}
