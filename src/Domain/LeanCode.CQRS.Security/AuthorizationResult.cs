namespace LeanCode.CQRS.Security
{
    public enum AuthorizationResult
    {
        Unauthenticated,
        InsufficientPermission,
        Authorized,
    }
}
