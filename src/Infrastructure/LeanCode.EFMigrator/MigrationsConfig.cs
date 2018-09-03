using System;

namespace LeanCode.EFMigrator
{
    public static class MigrationsConfig
    {
        public static string ConnectionStringKey = "ConnectionStrings:Database";

        public static string GetConnectionString()
        {
            var connStr = Environment.GetEnvironmentVariable(ConnectionStringKey);
            if (string.IsNullOrEmpty(connStr))
            {
                var keyLinux = ConnectionStringKey.Replace(":", "__");
                return Environment.GetEnvironmentVariable(keyLinux);
            }
            else
            {
                return connStr;
            }
        }
    }
}
