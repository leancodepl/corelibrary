namespace LeanCode.CQRS.OutputCaching.Registration;

internal static class TypeDistanceCalculator
{
    public static int GetDistance(Type from, Type to)
    {
        if (!to.IsAssignableFrom(from))
        {
            return -1;
        }

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

        return -1;
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

        return -1;
    }
}
