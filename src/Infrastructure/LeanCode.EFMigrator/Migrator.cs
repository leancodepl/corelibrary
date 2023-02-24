using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using static System.Console;
using static System.Environment;

namespace LeanCode.EFMigrator;

public abstract class Migrator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1819",
        Justification = "Convention for process arguments."
    )]
    protected virtual string[] Args { get; } = GetCommandLineArgs().Skip(1).ToArray();
    protected virtual string SeedPath { get; } = "Seed.sql";

    public virtual void Run()
    {
        var result = Run(Args);

        if (result != 0)
        {
            Exit(result);
        }
    }

    public virtual int Run(string[] args)
    {
        var migrate = false;
        var seed = false;

        foreach (var arg in args)
        {
            switch (arg)
            {
                case "migrate":
                case "--migrate":
                    migrate = true;
                    break;
                case "seed":
                case "--seed":
                    seed = true;
                    break;
                case "help":
                case "--help":
                    Usage(Out);
                    return 0;
                default:
                    Error.WriteLine($"Unknown argument: {arg}\n");
                    Usage(Error);
                    return 1;
            }
        }

        if (!(migrate || seed))
        {
            Error.WriteLine("No operations provided\n");
            Usage(Error);
            return 2;
        }

        var connectionString = MigrationsConfig.GetConnectionString();

        if (string.IsNullOrEmpty(connectionString))
        {
            Error.WriteLine("Cannot load connection string\n");
            Error.WriteLine("Environment variables");
            Error.WriteLine($"  {MigrationsConfig.ConnectionStringKey}");
            Error.WriteLine("and");
            Error.WriteLine($"  {MigrationsConfig.ConnectionStringKey.DenormalizeKey()}");
            Error.WriteLine("are unset or empty");
            return 3;
        }

        if (migrate)
        {
            MigrateAll();
        }

        if (seed)
        {
            Seed();
        }

        return 0;
    }

    protected virtual void Usage(TextWriter writer)
    {
        writer.WriteLine("Usage:");
        writer.WriteLine("  dotnet run migrate");
        writer.WriteLine("-or-");
        writer.WriteLine("  dotnet run seed");
        writer.WriteLine("-or-");
        writer.WriteLine("  dotnet run migrate seed");
    }

    protected abstract void MigrateAll();

    protected virtual void Migrate<TContext, TFactory>()
        where TFactory : IDesignTimeDbContextFactory<TContext>, new()
        where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;

        WriteLine($"Starting migration of {contextName}");

        using (var ctx = new TFactory().CreateDbContext(Args))
        {
            ctx.Database.Migrate();
        }

        WriteLine($"Migration of {contextName} completed");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2100", Justification = "It does not. :)")]
    protected virtual int Seed()
    {
        int rowsAffected;

        using (var connection = new SqlConnection(MigrationsConfig.GetConnectionString()))
        {
            connection.Open();

            using var command = new SqlCommand(File.ReadAllText(SeedPath), connection);

            rowsAffected = command.ExecuteNonQuery();
        }

        WriteLine($"Seed completed, rows affected: {rowsAffected}");

        return rowsAffected;
    }
}
