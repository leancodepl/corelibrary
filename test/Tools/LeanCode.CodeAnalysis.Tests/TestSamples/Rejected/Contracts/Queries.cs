using LeanCode.Contracts;

namespace LeanCode.CodeAnalysis.Tests.TestSamples.Rejected.Contracts;

public class FirstQuery : IQuery { }

public class SecondQuery : FirstQuery { }
