using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LeanCode.EFMigrator
{
    public abstract class Migrator
    {
        protected virtual string[] Args { get; } = Environment.GetCommandLineArgs().Skip(1).ToArray();

        public virtual void Run()
        {
            var isSeed = Args.Length == 1 && Args[0] == "seed";
            var isMigrate = Args.Length == 1 && Args[0] == "migrate";
            if (!(isSeed ^ isMigrate))
            {
                System.Console.WriteLine("Usage:");
                System.Console.WriteLine(" dotnet run (seed|migrate)");
                return;
            }
            else if (string.IsNullOrEmpty(MigrationsConfig.GetConnectionString()))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Cannot load connection string");
                return;
            }
            if (isSeed)
            {
                using (var conn = new SqlConnection(MigrationsConfig.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = File.ReadAllText("Seed.sql");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                MigrateAll();
            }
        }

        protected abstract void MigrateAll();

        protected virtual void Migrate<TContext, TFactory>()
            where TFactory : IDesignTimeDbContextFactory<TContext>, new()
            where TContext : DbContext
        {
            Console.WriteLine("Starting migration {0}", typeof(TContext).Name);
            var factory = new TFactory();
            using (var ctx = factory.CreateDbContext(Args))
            {
                ctx.Database.Migrate();
            }
            Console.WriteLine("Migration completed");
        }
    }
}
