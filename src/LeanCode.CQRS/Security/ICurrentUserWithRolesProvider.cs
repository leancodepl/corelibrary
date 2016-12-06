namespace LeanCode.CQRS.Security
{
    public interface ICurrentUserWithRolesProvider
    {
        ICurrentUserWithRoles GetCurrentUser();
    }
}
