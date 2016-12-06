using System;
using System.Collections.Generic;

namespace LeanCode.CQRS.Security
{
    public static class RoleRegistry
    {
        private static List<Role> allRoles = new List<Role>();

        public static IReadOnlyList<Role> All => allRoles;

        public static void Register(Role newRole)
        {
            if (newRole == null) throw new ArgumentNullException(nameof(newRole));
            allRoles.Add(newRole);
        }
    }
}
