namespace LeanCode.IdentityServer.KeyVault
{
    public class IdentityServerKeyVaultConfiguration
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string KeyId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
    }
}
