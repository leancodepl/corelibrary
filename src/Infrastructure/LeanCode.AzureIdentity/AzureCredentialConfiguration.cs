namespace LeanCode.AzureIdentity;

public record AzureCredentialConfiguration
{
    public string? ClientId { get; init; }
    public string? TenantId { get; init; }
    public string? ClientSecret { get; init; }
    public bool UseManagedIdentity { get; init; }
    public bool UseAzureCLI { get; init; }

    public static string TenantIdKey { get; set; } = "Azure:TenantId";
    public static string ClientIdKey { get; set; } = "Azure:ClientId";
    public static string ClientSecretKey { get; set; } = "Azure:ClientSecret";
    public static string UseManagedIdentityKey { get; set; } = "Azure:UseManagedIdentity";
    public static string UseAzureCLIKey { get; set; } = "Azure:UseAzureCLI";
}
