namespace LeanCode.CQRS.OutputCaching.Registration;

internal static class CQRSOutputCachePolicyName
{
    public static string For(Type objectType) => $"cqrs:{objectType.FullName}";
}
