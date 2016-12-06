using System.Collections.Generic;

namespace LeanCode.CQRS.Security
{
    public interface ICurrentUserWithRoles
    {
        IReadOnlyList<Role> Roles { get; }
        IReadOnlyList<string> Permissions { get; }
    }
}
