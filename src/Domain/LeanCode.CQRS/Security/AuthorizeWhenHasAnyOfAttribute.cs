namespace LeanCode.CQRS.Security
{
    public interface HasPermissions
    { }

    public class AuthorizeWhenHasAnyOfAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenHasAnyOfAttribute(params string[] permissions)
            : base(typeof(HasPermissions), permissions)
        { }
    }
}
