using System;
using Xunit;
using Xunit.Sdk;

namespace LeanCode.Test.Helpers
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
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
}
