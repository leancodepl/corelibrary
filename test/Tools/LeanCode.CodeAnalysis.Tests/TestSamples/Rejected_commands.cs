using LeanCode.Contracts;

namespace LeanCode.CodeAnalysis.Tests.Data;

public class FirstRejectedCommand : ICommand { }

public class SecondRejectedCommand : FirstRejectedCommand { }
