using LeanCode.CQRS.Security;

namespace LeanCode.IntegrationTests.App;

public class AppRoles : IRoleRegistration
{
    public IEnumerable<Role> Roles { get; } = [new Role("user", "user")];
}
