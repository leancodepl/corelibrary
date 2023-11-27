using LeanCode.Contracts;

namespace LeanCode.CodeAnalysis.Tests.TestSamples.Rejected.Contracts;

public class FirstCommand : ICommand { }

public class SecondCommand : FirstCommand { }
