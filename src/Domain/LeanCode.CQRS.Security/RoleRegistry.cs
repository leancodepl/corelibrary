using System.Collections.Generic;
using System.Linq;

namespace LeanCode.CQRS.Security
{
    public sealed class RoleRegistry
    {
        public IReadOnlyList<Role> All { get; }

        public RoleRegistry(IEnumerable<IRoleRegistration> registrations)
        {
            All = registrations.SelectMany(r => r.Roles).ToArray();
        }
    }
}
