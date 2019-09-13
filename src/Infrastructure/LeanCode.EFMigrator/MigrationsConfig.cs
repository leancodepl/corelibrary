using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace LeanCode.EFMigrator
{
    public static class MigrationsConfig
    {
        public static string ConnectionStringKey { get; set; } = "ConnectionStrings:Database";
        public static string KeyVaultUrlKey { get; set; } = "Secrets:KeyVault:VaultUrl";
        public static string KeyVaultClientIdKey { get; set; } = "Secrets:KeyVault:ClientId";
        public static string KeyVaultClientSecretKey { get; set; } = "Secrets:KeyVault:ClientSecret";

        private static string? azureKeyVaultConnectionString;

        private static async Task<string> GetAzureKeyVaultAccessTokenAsync(
            string authority, string resource, string scope)
        {
            string clientId = GetEnvironmentVariable(KeyVaultClientIdKey)
                ?? throw new ArgumentNullException(KeyVaultClientIdKey);

            string clientSecret = GetEnvironmentVariable(KeyVaultClientSecretKey)
                ?? throw new ArgumentNullException(KeyVaultClientSecretKey);

            var context = new AuthenticationContext(authority);
            var credential = new ClientCredential(clientId, clientSecret);

            var token = await context.AcquireTokenAsync(resource, credential)
                .ConfigureAwait(false);

            return token.AccessToken;
        }

        public static async Task UseConnectionStringFromAzureKeyVaultAsync()
        {
            string keyVaultUrl = GetEnvironmentVariable(KeyVaultUrlKey)
                ?? throw new ArgumentNullException(KeyVaultUrlKey);

            string connectionStringKey = ConnectionStringKey.Replace(":", "--");

            using (var client = new KeyVaultClient(GetAzureKeyVaultAccessTokenAsync))
            {
                var secret = await client.GetSecretAsync(keyVaultUrl, connectionStringKey)
                    .ConfigureAwait(false);

                azureKeyVaultConnectionString = secret.Value;
            }
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
