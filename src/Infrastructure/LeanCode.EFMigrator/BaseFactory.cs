using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeanCode.EFMigrator;

public abstract class BaseFactory<TContext, TFactory> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
    where TFactory : BaseFactory<TContext, TFactory>
{
    protected virtual string AssemblyName =>
        typeof(TFactory).Assembly.GetName().Name
        ?? throw new InvalidOperationException("This type is not supported on Assembly-less runtimes.");

    protected virtual void UseAdditionalDbContextOptions(DbContextOptionsBuilder<TContext> builder) { }

    public TContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<TContext>().UseLoggerFactory(
            new ServiceCollection()
                .AddLogging(cfg => cfg.AddConsole())
                .BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>()
        );

        builder = UseDbProvider(builder);

        UseAdditionalDbContextOptions(builder);

        return (TContext?)Activator.CreateInstance(typeof(TContext), builder.Options)
            ?? throw new InvalidOperationException("Failed to create DbContext instance.");
    }

    public abstract DbContextOptionsBuilder<TContext> UseDbProvider(DbContextOptionsBuilder<TContext> builder);
}
