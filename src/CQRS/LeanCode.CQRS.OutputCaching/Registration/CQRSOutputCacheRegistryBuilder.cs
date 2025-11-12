using System.Reflection;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.BasePolicies;

namespace LeanCode.CQRS.OutputCaching.Registration;

public sealed class CQRSOutputCacheRegistryBuilder
{
    private readonly Dictionary<Type, CQRSOutputCacheRegistry.PolicyDescriptor> descriptors = new();
    private readonly List<CQRSOutputCacheRegistry.PolicyDescriptor> descriptorList = [];

    public static IEnumerable<(Type ContractType, Type PolicyType)> EnumeratePolicies(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(
                t =>
                    t.ImplementedInterfaces.Where(i =>
                        i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(ICQRSOutputCachePolicy<>)
                    ),
                (t, i) => (ContractType: i.GenericTypeArguments[0], PolicyType: t.AsType())
            );
    }

    public static bool IsValidPolicyType(Type policyType)
    {
        return policyType
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICQRSOutputCachePolicy<>));
    }

    public void Register(Type contractType, Type policyType)
    {
        ArgumentNullException.ThrowIfNull(contractType);
        ArgumentNullException.ThrowIfNull(policyType);

        if (!IsValidPolicyType(policyType))
        {
            throw new ArgumentException(
                $"Type {policyType.FullName} must implement ICQRSOutputCachePolicy<>.",
                nameof(policyType)
            );
        }

        var descriptor = new CQRSOutputCacheRegistry.PolicyDescriptor(contractType, policyType);

        if (!descriptors.TryAdd(contractType, descriptor))
        {
            throw new InvalidOperationException(
                $"Multiple caching policies for {contractType.FullName} are specified: Policy 1 - {descriptors[contractType].PolicyType.FullName}, Policy 2 - {policyType.FullName}."
            );
        }

        descriptorList.Add(descriptor);
    }

    public CQRSOutputCacheRegistry Build(IEnumerable<CQRSObjectMetadata> objects)
    {
        ArgumentNullException.ThrowIfNull(objects);

        var precomputed = objects
            .Where(o =>
                o.ObjectKind is CQRSObjectKind.Query or CQRSObjectKind.Operation
                && o.ObjectType.GetCustomAttribute<CacheOutputAttribute>() is not null
            )
            .Select(o => new KeyValuePair<Type, CQRSOutputCacheRegistry.PolicyDescriptor>(
                o.ObjectType,
                Resolve(o.ObjectType)
            ))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return new(precomputed);
    }

    private CQRSOutputCacheRegistry.PolicyDescriptor Resolve(Type contractType)
    {
        if (descriptors.TryGetValue(contractType, out var exact))
        {
            return exact;
        }

        CQRSOutputCacheRegistry.PolicyDescriptor? best = null;
        CQRSOutputCacheRegistry.PolicyDescriptor? otherEqualToBest = null;

        foreach (var candidate in descriptorList)
        {
            if (!candidate.ContractType.IsAssignableFrom(contractType))
            {
                continue;
            }

            if (best is null)
            {
                best = candidate;
                continue;
            }

            var comparison = TypeDistanceComparer.Compare(best.ContractType, candidate.ContractType, contractType);

            switch (comparison)
            {
                case TypeDistanceComparison.FirstCloser:
                    break;
                case TypeDistanceComparison.SecondCloser:
                    best = candidate;
                    otherEqualToBest = null;
                    break;
                case TypeDistanceComparison.Equal:
                    otherEqualToBest = candidate;
                    break;
            }
        }

        if (best is null)
        {
            throw new InvalidOperationException($"No output cache policy registered for {contractType.FullName}.");
        }

        if (otherEqualToBest is not null)
        {
            throw new InvalidOperationException(
                $"Multiple output cache policies match {contractType.FullName} at the same specificity: {best.ContractType.FullName} and {otherEqualToBest.ContractType.FullName}."
            );
        }

        return best;
    }
}
