using System.Diagnostics.CodeAnalysis;

namespace LeanCode.CQRS.Security
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1302:InterfaceNamesMustBeginWithI", Justification = "Authorizer interfaces are exempt.")]
    public interface HasPermissions
    { }

    public class AuthorizeWhenHasAnyOfAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenHasAnyOfAttribute(params string[] permissions)
            : base(typeof(HasPermissions), permissions)
        { }
    }
}
