using System.Collections.Generic;
using LeanCode.CQRS.Security;

namespace LeanCode.Example.Security
{
    using Reg = Roles;

    public static class Roles
    {
        public const string User = "leancode/user";
        public const string Admin = "leancode/admin";
    }

    public static class Permissions
    {
        public const string View = "leancode/view";
        public const string Write = "leancode/write";
    }

    public class AppRoles : IRoleRegistration
    {
        public IEnumerable<Role> Roles { get; } = new Role[]
        {
            new Role(
                Reg.User, Permissions.View),
            new Role(
                Reg.Admin, Permissions.Write),
        };
    }
}
