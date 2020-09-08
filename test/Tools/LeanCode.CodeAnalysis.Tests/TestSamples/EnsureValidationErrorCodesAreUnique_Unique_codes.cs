using LeanCode.CQRS;

namespace LeanCode.CodeAnalysis.Tests.TestSamples
{
    public class Dto
    {
        public class ErrorCodes
        {
            public const int Code1 = 1;
        }
    }

    public class Cmd1 : ICommand
    {
        public class ErrorCodes
        {
            public const int Code2 = 2;
        }
    }
}
