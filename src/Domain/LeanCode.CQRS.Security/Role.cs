using System;
using System.Collections.Generic;

namespace LeanCode.CQRS.Security
{
    public class Role
    {
        public string Name { get; }
        public HashSet<string> Permissions { get; }

        public Role(string name, params string[] permissions)
        {
            Validate(name, permissions);

            this.Name = name;
            this.Permissions = new HashSet<string>(permissions);
        }

        public Role(string name, IEnumerable<string> permissions)
        {
            Validate(name, permissions);

            this.Name = name;
            this.Permissions = new HashSet<string>(permissions);
        }

        private static void Validate(string name, IEnumerable<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(
                    "Name must be specified.", nameof(name));
            }
            if (permissions == null)
            {
                throw new ArgumentNullException(
                    "Permissions must be non-null", nameof(permissions));
            }
        }
    }
}
