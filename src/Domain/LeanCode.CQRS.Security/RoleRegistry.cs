using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LeanCode.CQRS.Security;

public sealed class RoleRegistry
{
    public ImmutableList<Role> All { get; }

    public RoleRegistry(IEnumerable<IRoleRegistration> registrations)
    {
        All = registrations.SelectMany(r => r.Roles).ToImmutableList();
    }
}
