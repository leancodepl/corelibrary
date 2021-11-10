using LeanCode.CQRS;
using LeanCode.CQRS.Security;

namespace LeanCode.CodeAnalysis.Tests.Data
{
    public abstract class BaseCommand : ICommand { }

    [AuthorizeWhenHasAnyOf("Some_permission")]
    public class FirstCommand : BaseCommand { }

    public class SecondCommand : FirstCommand { }

    [AuthorizeWhenCustom]
    public class ThirdCommand : ICommand { }

    [AllowUnauthorized]
    public class UnauthorizedCommand : ICommand { }
}
