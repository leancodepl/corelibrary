namespace LeanCode.CQRS.Security
{
    public enum AuthorizationResult
    {
        Unauthenticated,
        InsufficientPermission,
        Authorized
    }

    public interface IAuthorizer
    {
        AuthorizationResult CheckIfAuthorized<T>(T obj);
    }
}
