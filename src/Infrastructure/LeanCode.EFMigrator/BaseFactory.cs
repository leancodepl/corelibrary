using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeanCode.EFMigrator
{
    public abstract class BaseFactory<TContext, TFactory> : IDesignTimeDbContextFactory<TContext>
        where TContext : DbContext
        where TFactory : BaseFactory<TContext, TFactory>
    {
        protected virtual string AssemblyName => typeof(TFactory).Assembly.GetName().Name
            ?? throw new ArgumentNullException(nameof(AssemblyName));

        protected virtual void UseAdditionalSqlServerDbContextOptions(SqlServerDbContextOptionsBuilder builder) { }

        public TContext CreateDbContext(string[] args)
        {
            return (TContext?)Activator.CreateInstance(
                typeof(TContext),
                new DbContextOptionsBuilder<TContext>()
                    .UseLoggerFactory(new ServiceCollection()
                        .AddLogging(cfg => cfg.AddConsole())
                        .BuildServiceProvider()
                        .GetRequiredService<ILoggerFactory>())
                    .UseSqlServer(
                        MigrationsConfig.GetConnectionString(),
                        cfg => UseAdditionalSqlServerDbContextOptions(
                            cfg.MigrationsAssembly(AssemblyName)))
                    .Options) ?? throw new ArgumentNullException();
        }
    }
}
