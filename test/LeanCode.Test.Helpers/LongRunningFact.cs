using Xunit;

namespace LeanCode.Test.Helpers;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class LongRunningFact : FactAttribute
{
#if EXCLUDE_LONG_RUNNING_TESTS
    public LongRunningFact()
    {
        Skip = "Long running test";
    }
#else
    public LongRunningFact() { }
#endif
}
