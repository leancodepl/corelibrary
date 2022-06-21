using LeanCode.Contracts;
using LeanCode.Contracts.Security;

namespace LeanCode.CodeAnalysis.Tests.TestSamples
{
    public abstract class BaseOperation : IOperation { }

    [AuthorizeWhenHasAnyOf("Sample_permission")]
    public class FirstOperation : BaseOperation { }

    public class SecondOperation : FirstOperation { }

    [AuthorizeWhenHasAnyOf("Sample_permission")]
    public class ThirdOperation : IOperation { }

    [AllowUnauthorized]
    public class UnauthorizedOperation : IOperation { }
}
