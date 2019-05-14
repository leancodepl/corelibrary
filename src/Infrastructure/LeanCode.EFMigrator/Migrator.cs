using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using static System.Console;
using static System.Environment;

namespace LeanCode.EFMigrator
{
    public abstract class Migrator
    {
        protected virtual string[] Args { get; } = GetCommandLineArgs().Skip(1).ToArray();

        public virtual void Run()
        {
            int result = Run(Args);

            if (result != 0)
            {
                Exit(result);
            }
        }

        public virtual int Run(string[] args)
        {
            bool migrate = false;
            bool seed = false;

            foreach (string arg in args)
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

            string connectionString = MigrationsConfig.GetConnectionString();

            if (string.IsNullOrEmpty(connectionString))
            {
                Error.WriteLine("Cannot load connection string");
                Error.WriteLine($"Environment variable `{MigrationsConfig.ConnectionStringKey}` is unset or empty");
                return 3;
            }

            if (migrate)
            {
                MigrateAll();
            }

            if (seed)
            {
                Seed(connectionString, null);
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
            WriteLine($"Starting migration {typeof(TContext).Name}");

            using (var ctx = new TFactory().CreateDbContext(Args))
            {
                ctx.Database.Migrate();
            }

            WriteLine("Migration completed");
        }

        protected virtual int Seed(string connectionString, string seed)
        {
            int rowsAffected;

            using (var conn = new SqlConnection(connectionString ?? MigrationsConfig.GetConnectionString()))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = File.ReadAllText(seed ?? "Seed.sql");

                    rowsAffected = cmd.ExecuteNonQuery();
                }

                conn.Close();
            }

            WriteLine("Seed completed");

            return rowsAffected;
        }
    }
}
