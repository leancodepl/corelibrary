using System.Collections.Generic;
using LeanCode.CQRS.Security;

namespace LeanCode.IntegrationTestHelpers.Tests.App
{
    public class AppRoles : IRoleRegistration
    {
        public IEnumerable<Role> Roles { get; } = new Role[]
        {
            new Role("user", "user"),
        };
    }
}
