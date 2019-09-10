namespace LeanCode.CQRS.Security
{
    [System.Obsolete]
    public enum AuthorizationResult
    {
        Unauthenticated,
        InsufficientPermission,
        Authorized,
    }
}
