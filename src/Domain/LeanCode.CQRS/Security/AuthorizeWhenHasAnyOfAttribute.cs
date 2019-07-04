namespace LeanCode.CQRS.Security
{
#pragma warning disable IDE1006
    public interface HasPermissions
    { }
#pragma warning restore IDE1006

    public class AuthorizeWhenHasAnyOfAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenHasAnyOfAttribute(params string[] permissions)
            : base(typeof(HasPermissions), permissions)
        { }
    }
}
