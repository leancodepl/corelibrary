using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.BasePolicies;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.Registration;

public sealed class CQRSOutputCacheRegistry : ICQRSEndpointMetadataProvider
{
    public ImmutableDictionary<Type, PolicyDescriptor> PolicyForObject { get; }

    public CQRSOutputCacheRegistry(IReadOnlyDictionary<Type, PolicyDescriptor> policyForObject)
    {
        PolicyForObject = policyForObject.ToImmutableDictionary();
    }

    public IEnumerable<object> GetAdditionalEndpointMetadata(CQRSObjectMetadata metadata)
    {
        if (
            metadata.ObjectKind == CQRSObjectKind.Command
            || metadata.ObjectType.GetCustomAttribute<CacheOutputAttribute>() is null
        )
        {
            return [];
        }

        if (!PolicyForObject.TryGetValue(metadata.ObjectType, out var descriptor))
        {
            throw new InvalidOperationException(
                $"No output cache policy registered for {metadata.ObjectType.FullName}."
            );
        }

        var policyName = CQRSOutputCachePolicyName.For(descriptor.PolicyType);
        return [new OutputCacheAttribute { PolicyName = policyName }];
    }



    public sealed record PolicyDescriptor(Type ContractType, Type PolicyType);
}
