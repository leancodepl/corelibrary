using LeanCode.CQRS;

namespace LeanCode.CodeAnalysis.Tests.TestSamples
{
    public class FirstRejectedQuery : IQuery
    { }

    public class SecondRejectedQuery : FirstRejectedQuery
    { }
}
