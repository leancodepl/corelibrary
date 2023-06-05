using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LeanCode.CQRS.Security;

public class Role
{
    public string Name { get; }
    public ImmutableHashSet<string> Permissions { get; }

    public Role(string name, params string[] permissions)
    {
        Validate(name);

        Name = name;
        Permissions = ImmutableHashSet.Create(permissions);
    }

    public Role(string name, IEnumerable<string> permissions)
    {
        Validate(name);

        Name = name;
        Permissions = permissions.ToImmutableHashSet();
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be specified.", nameof(name));
        }
    }
}
