using System.Reflection;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.BasePolicies;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.Registration;

internal sealed class CQRSOutputCacheRegistry : ICQRSEndpointMetadataProvider
{
    private readonly Dictionary<Type, Type> policyForObject = new();
    public IReadOnlyDictionary<Type, Type> PolicyForObject => policyForObject;

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

    private static bool IsValidPolicyCQRSObject(Type cqrsObjectType)
    {
        return cqrsObjectType
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>) || i.GetGenericTypeDefinition() == typeof(IOperation<>));
    }

    private static bool IsValidPolicyType(Type policyType)
    {
        return policyType
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICQRSOutputCachePolicy<>));
    }

    internal void Register(Type contractType, Type policyType)
    {
        ArgumentNullException.ThrowIfNull(contractType);
        ArgumentNullException.ThrowIfNull(policyType);

        if (!IsValidPolicyCQRSObject(contractType))
        {
            throw new ArgumentException(
                $"Type {contractType.FullName} must implement IQuery<> or IOperation<>.",
                nameof(contractType)
            );
        }

        if (!IsValidPolicyType(policyType))
        {
            throw new ArgumentException(
                $"Type {policyType.FullName} must implement ICQRSOutputCachePolicy<>.",
                nameof(policyType)
            );
        }

        if (!policyForObject.TryAdd(contractType, policyType))
        {
            throw new InvalidOperationException(
                $"Multiple caching policies for {contractType.FullName} are specified: Policy 1 - {policyForObject[contractType].FullName}, Policy 2 - {policyType.FullName}."
            );
        }
    }

    public IEnumerable<object> GetAdditionalEndpointMetadata(CQRSObjectMetadata metadata)
    {
        if (!PolicyForObject.TryGetValue(metadata.ObjectType, out var policy))
        {
            return [];
        }

        var policyName = CQRSOutputCachePolicyName.For(policy);
        return [new OutputCacheAttribute { PolicyName = policyName }];
    }
}
