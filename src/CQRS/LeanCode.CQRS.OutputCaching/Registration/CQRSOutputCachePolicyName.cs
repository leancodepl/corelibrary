namespace LeanCode.CQRS.OutputCaching.Registration;

public static class CQRSOutputCachePolicyName
{
    private const string Prefix = "cqrs:";

    public static string For(Type objectType) => $"{Prefix}{objectType.FullName}";

    public static string GetObjectTypeString(string policyName) => policyName[Prefix.Length..];
}
