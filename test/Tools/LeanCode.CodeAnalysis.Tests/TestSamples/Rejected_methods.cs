namespace LeanCode.CodeAnalysis.Tests.Data;

public class RejectedMethods
{
    public virtual CancellationToken CancellationTokenNamingConvention(CancellationToken ct)
    {
        return ct;
    }
}
