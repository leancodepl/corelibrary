namespace LeanCode.CQRS.Security
{
    public abstract class PermissionAuthorizer : CustomAuthorizer<object, string[]>
    { }

    public class AuthorizeWhenHasAnyOfAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenHasAnyOfAttribute(params string[] Permissions)
            : base(typeof(PermissionAuthorizer), Permissions)
        { }
    }
}
