using System;

namespace LeanCode.EFMigrator
{
    public static class MigrationsConfig
    {
        public static string ConnectionStringKey = "ConnectionStrings:Database";
        public static string ConnectionStringDenormalizedKey => ConnectionStringKey.Replace(":", "__");

        public static string GetConnectionString()
        {
            var connectionString = Environment.GetEnvironmentVariable(ConnectionStringKey);

            if (string.IsNullOrEmpty(connectionString))
            {
                return Environment.GetEnvironmentVariable(ConnectionStringDenormalizedKey);
            }
            else
            {
                return connectionString;
            }
        }
    }
}
