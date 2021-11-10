using System.Diagnostics.CodeAnalysis;

namespace LeanCode.CQRS.Security
{
    [SuppressMessage("?", "CA1715", Justification = "Authorizer interfaces are exempt.")]
    [SuppressMessage("?", "CA1040", Justification = "Marker interface.")]
    public interface HasPermissions { }

    public sealed class AuthorizeWhenHasAnyOfAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenHasAnyOfAttribute(params string[] permissions)
               : base(typeof(HasPermissions), permissions) { }
    }
}
