using Azure.Security.KeyVault.Secrets;
using LeanCode.AzureIdentity;

namespace LeanCode.EFMigrator;

public static class MigrationsConfig
{
    public static string ConnectionStringKey { get; set; } = "SqlServer:ConnectionString";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1056", Justification = "It should be `string`.")]
    public static string KeyVaultUrlKey { get; set; } = "KeyVault:VaultUrl";

    private static string? azureKeyVaultConnectionString;

    public static async Task UseConnectionStringFromAzureKeyVaultAsync()
    {
        var keyVaultUrl = GetEnvironmentVariable(KeyVaultUrlKey) ?? throw new ArgumentNullException(KeyVaultUrlKey);

        var connectionStringKey = ConnectionStringKey.Replace(":", "--", StringComparison.Ordinal);

        var credential = DefaultLeanCodeCredential.CreateFromEnvironment();
        var client = new SecretClient(new(keyVaultUrl), credential);

        var secret = await client.GetSecretAsync(connectionStringKey);
        azureKeyVaultConnectionString = secret.Value.Value;
    }

    public static string? GetConnectionString() =>
        azureKeyVaultConnectionString ?? GetEnvironmentVariable(ConnectionStringKey);

    public static string DenormalizeKey(this string @this) => @this.Replace(":", "__", StringComparison.Ordinal);

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
