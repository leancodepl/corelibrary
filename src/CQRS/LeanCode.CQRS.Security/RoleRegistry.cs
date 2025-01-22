using System.Collections.Immutable;

namespace LeanCode.CQRS.Security;

public sealed class RoleRegistry
{
    public ImmutableList<Role> All { get; }

    public RoleRegistry(IEnumerable<IRoleRegistration> registrations)
    {
        All = registrations.SelectMany(r => r.Roles).ToImmutableList();
    }
}
