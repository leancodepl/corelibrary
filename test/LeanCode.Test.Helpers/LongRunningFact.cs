using System.Runtime.CompilerServices;
using Xunit;
using Xunit.v3;

namespace LeanCode.Test.Helpers;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class LongRunningFact : FactAttribute, ITraitAttribute
{
    public LongRunningFact([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        : base(sourceFilePath, sourceLineNumber) { }

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() => [new("category", "long-running")];
}

public class LongRunningFactTest
{
    [LongRunningFact]
    public void Long_running_test() { }
}
