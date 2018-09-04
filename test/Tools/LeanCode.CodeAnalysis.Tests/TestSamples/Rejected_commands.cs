using LeanCode.CQRS;

namespace LeanCode.CodeAnalysis.Tests.Data
{
    public class FirstRejectedCommand : ICommand
    { }

    public class SecondRejectedCommand : FirstRejectedCommand
    { }
}
