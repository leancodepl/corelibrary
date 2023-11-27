using LeanCode.Contracts;

namespace LeanCode.CodeAnalysis.Tests.TestSamples.Rejected.Contracts;

public class FirstOperation : IOperation { }

public class SecondOperation : FirstOperation { }
