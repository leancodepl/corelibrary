namespace LeanCode.CQRS.OutputCaching.Registration;

internal static class TypeDistanceComparer
{
    public static TypeDistanceComparison Compare(Type from1, Type from2, Type to)
    {
        ArgumentNullException.ThrowIfNull(from1);
        ArgumentNullException.ThrowIfNull(from2);
        ArgumentNullException.ThrowIfNull(to);

        if (!from1.IsAssignableFrom(to))
        {
            throw new ArgumentException($"{from1.FullName} is not assignable from {to.FullName}.", nameof(from1));
        }

        if (!from2.IsAssignableFrom(to))
        {
            throw new ArgumentException($"{from2.FullName} is not assignable from {to.FullName}.", nameof(from2));
        }

        if (from1 == from2)
        {
            return TypeDistanceComparison.Equal;
        }

        var from1AssignableFrom2 = from1.IsAssignableFrom(from2);
        var from2AssignableFrom1 = from2.IsAssignableFrom(from1);

        if (from1AssignableFrom2 && !from2AssignableFrom1)
        {
            return TypeDistanceComparison.SecondCloser;
        }

        if (from2AssignableFrom1 && !from1AssignableFrom2)
        {
            return TypeDistanceComparison.FirstCloser;
        }

        return TypeDistanceComparison.Equal;
    }

    private static int GetDistance(Type from, Type to)
    {
        if (from == to)
        {
            return 0;
        }

        return to.IsInterface ? InterfaceDistance(from, to) : ClassDistance(from, to);
    }

    private static int ClassDistance(Type from, Type to)
    {
        var current = from;
        var distance = 0;

        while (current is not null)
        {
            if (current == to)
            {
                return distance;
            }

            current = current.BaseType;
            distance++;
        }

        return int.MaxValue;
    }

    private static int InterfaceDistance(Type from, Type to)
    {
        var visited = new HashSet<Type>();
        var queue = new Queue<(Type Type, int Distance)>();
        queue.Enqueue((from, 0));
        visited.Add(from);

        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();
            foreach (var iface in current.GetInterfaces())
            {
                if (!visited.Add(iface))
                {
                    continue;
                }

                if (iface == to)
                {
                    return distance + 1;
                }

                queue.Enqueue((iface, distance + 1));
            }

            if (current.BaseType is { } baseType && visited.Add(baseType))
            {
                if (baseType == to)
                {
                    return distance + 1;
                }

                queue.Enqueue((baseType, distance + 1));
            }
        }

        return int.MaxValue;
    }
}

internal enum TypeDistanceComparison
{
    FirstCloser = -1,
    Equal = 0,
    SecondCloser = 1,
}
