using System.Reflection;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.BasePolicies;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.Registration;

internal sealed class CQRSOutputCacheRegistry : ICQRSEndpointMetadataProvider
{
    private readonly Dictionary<Type, Type> policyForObject = [];
    public IReadOnlyDictionary<Type, Type> PolicyForObject => policyForObject;

    public static IEnumerable<(Type ContractType, Type PolicyType)> EnumeratePolicies(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(
                t =>
                    GetImplementedCQRSOutputCachePolicies(t.AsType())
                        .Where(i => IsValidPolicyCQRSObject(i.GenericTypeArguments[0])),
                (t, i) => (ContractType: i.GenericTypeArguments[0], PolicyType: t.AsType())
            );
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

    internal void Register(Type contractType, Type policyType)
    {
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

    private static bool IsValidPolicyCQRSObject(Type cqrsObjectType)
    {
        return cqrsObjectType
            .GetInterfaces()
            .Any(i =>
                i.IsConstructedGenericType
                && (
                    i.GetGenericTypeDefinition() == typeof(IQuery<>)
                    || i.GetGenericTypeDefinition() == typeof(IOperation<>)
                )
            );
    }

    private static bool IsValidPolicyType(Type policyType)
    {
        return GetImplementedCQRSOutputCachePolicies(policyType).Any();
    }

    private static IEnumerable<Type> GetImplementedCQRSOutputCachePolicies(Type policyType)
    {
        return policyType
            .GetInterfaces()
            .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(ICQRSOutputCachePolicy<>));
    }
}
