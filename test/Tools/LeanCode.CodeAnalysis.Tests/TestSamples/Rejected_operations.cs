using LeanCode.Contracts;

namespace LeanCode.CodeAnalysis.Tests.TestSamples;

public class FirstRejectedOperation : IOperation { }

public class SecondRejectedOperation : FirstRejectedOperation { }
