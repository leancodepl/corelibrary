using LeanCode.Contracts;
using LeanCode.Contracts.Security;

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

    public sealed class AuthorizeWhenCustomAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenCustomAttribute(Type authorizerType = null)
               : base(authorizerType ?? typeof(object))
        { }
    }
}
