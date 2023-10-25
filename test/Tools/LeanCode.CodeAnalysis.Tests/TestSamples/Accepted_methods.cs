namespace LeanCode.CodeAnalysis.Tests.Data;

public static class AcceptedMethods
{
    public static CancellationToken CancellationTokenNamingConvention(CancellationToken cancellationToken)
    {
        return cancellationToken;
    }
}

// Methods with `override` and `new` keywords are accepted as they might come from external source
public class AcceptedOverrideMethod : RejectedMethods
{
    public override CancellationToken CancellationTokenNamingConvention(CancellationToken ct)
    {
        return ct;
    }
}

public class AcceptedNewMethod : RejectedMethods
{
    public static new CancellationToken CancellationTokenNamingConvention(CancellationToken ct)
    {
        return ct;
    }
}
