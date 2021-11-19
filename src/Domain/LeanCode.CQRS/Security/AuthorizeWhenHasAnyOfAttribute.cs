using System.Diagnostics.CodeAnalysis;

namespace LeanCode.CQRS.Security
{
    [SuppressMessage("?", "CA1040", Justification = "Marker interface.")]
    public interface IHasPermissions { }

    public sealed class AuthorizeWhenHasAnyOfAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenHasAnyOfAttribute(params string[] permissions)
            : base(typeof(IHasPermissions), permissions) { }
    }
}
