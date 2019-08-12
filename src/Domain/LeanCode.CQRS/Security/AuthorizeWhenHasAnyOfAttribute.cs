namespace LeanCode.CQRS.Security
{
#pragma warning disable IDE1006
    public interface HasPermissions
    { }
#pragma warning restore IDE1006

    /// <summary>
    /// Specifies a set of permissions user needs to have to execute command/query.
    /// User needs only one of the specified permissions (as in OR clause).
    /// </summary>
    /// <remarks>
    /// If multiple permissions are needed to authorize a user, multiple <c>AuthorizeWhenHasAnyOf</c> attributes can be used
    /// </remarks>
    public class AuthorizeWhenHasAnyOfAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenHasAnyOfAttribute(params string[] permissions)
            : base(typeof(HasPermissions), permissions)
        { }
    }
}
