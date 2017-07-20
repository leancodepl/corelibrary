using System;
using System.Collections.Generic;

namespace LeanCode.CQRS.Security
{
    public class Role
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public HashSet<string> Permissions { get; private set; }

        private Role() { }

        public static Role New(Guid id, string name, HashSet<string> permissions)
        {
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Parameter cannot be null or empty", nameof(name));
            if (id == Guid.Empty)
                throw new ArgumentException("Parameter cannot be empty", nameof(id));

            return new Role()
            {
                Id = id,
                Name = name,
                Permissions = permissions
            };
        }
    }
}
