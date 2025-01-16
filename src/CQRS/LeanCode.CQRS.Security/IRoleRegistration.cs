namespace LeanCode.CQRS.Security;

public interface IRoleRegistration
{
    IEnumerable<Role> Roles { get; }
}
