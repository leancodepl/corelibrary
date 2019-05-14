using System;
using System.Runtime.InteropServices;

namespace LeanCode.EFMigrator
{
    public static class MigrationsConfig
    {
        public static readonly string ConnectionStringKeyWindows = "ConnectionStrings:Database";
        public static readonly string ConnectionStringKeyOther = "ConnectionStrings__Database";

        public static string ConnectionStringKey =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ConnectionStringKeyWindows
                : ConnectionStringKeyOther;

        public static string GetConnectionString() =>
            Environment.GetEnvironmentVariable(ConnectionStringKey);
    }
}
