using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace LeanCode.EFMigrator
{
    public static class MigrationsConfig
    {
        public static string ConnectionStringKey { get; set; } = "ConnectionStrings:Database";
        public static string KeyVaultUrlKey { get; set; } = "Secrets:KeyVault:VaultUrl";

        private static string? azureKeyVaultConnectionString;

        public static async Task UseConnectionStringFromAzureKeyVaultAsync()
        {
            var keyVaultUrl = GetEnvironmentVariable(KeyVaultUrlKey)
                ?? throw new ArgumentNullException(KeyVaultUrlKey);

            var connectionStringKey = ConnectionStringKey.Replace(":", "--");

            var client = new SecretClient(new(keyVaultUrl), new DefaultAzureCredential());

            var secret = await client.GetSecretAsync(connectionStringKey);
            azureKeyVaultConnectionString = secret.Value.Value;
        }

        public static string? GetConnectionString() =>
            azureKeyVaultConnectionString ?? GetEnvironmentVariable(ConnectionStringKey);

        public static string DenormalizeKey(this string @this) => @this.Replace(":", "__");

        private static string? GetEnvironmentVariable(string key)
        {
            var variable = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrEmpty(variable))
            {
                return Environment.GetEnvironmentVariable(key.DenormalizeKey());
            }
            else
            {
                return variable;
            }
        }
    }
}
