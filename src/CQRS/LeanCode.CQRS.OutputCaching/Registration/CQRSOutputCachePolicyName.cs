namespace LeanCode.CQRS.OutputCaching.Registration;

internal static class CQRSOutputCachePolicyName
{
    public static string For(Type objectType)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        return $"cqrs:{objectType.FullName}";
    }
}
