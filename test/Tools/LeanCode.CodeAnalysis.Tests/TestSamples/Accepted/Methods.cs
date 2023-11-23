namespace LeanCode.CodeAnalysis.Tests.TestSamples.Accepted;

public static class Methods
{
    public static CancellationToken CancellationTokenNamingConvention(CancellationToken cancellationToken)
    {
        return cancellationToken;
    }
}

// Methods with `override` and `new` keywords are accepted as they might come from external source
public class OverrideMethod : Rejected.Methods
{
    public override CancellationToken CancellationTokenNamingConvention(CancellationToken ct)
    {
        return ct;
    }
}

public class AcceptedNewMethod : Rejected.Methods
{
    public static new CancellationToken CancellationTokenNamingConvention(CancellationToken ct)
    {
        return ct;
    }
}
