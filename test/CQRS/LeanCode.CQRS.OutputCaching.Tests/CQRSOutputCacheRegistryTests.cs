using FluentAssertions;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.Registration;
using Microsoft.AspNetCore.OutputCaching;
using Xunit;

namespace LeanCode.CQRS.OutputCaching.Tests;

public class CQRSOutputCacheRegistryTests
{
    private static readonly ObjectExecutor Executor = (_, _) => Task.FromResult<object?>(null);

    [Fact]
    public void EnumeratePolicies_finds_cqrs_policies_in_assemblies()
    {
        var policies = CQRSOutputCacheRegistry.EnumeratePolicies([typeof(TestQueryOCP).Assembly]);

        policies
            .Should()
            .ContainSingle(p => p.ContractType == typeof(TestQuery) && p.PolicyType == typeof(TestQueryOCP));
    }

    [Fact]
    public void EnumeratePolicies_ignores_non_cqrs_policies_in_assemblies()
    {
        var policies = CQRSOutputCacheRegistry.EnumeratePolicies([typeof(TestQueryOCP).Assembly]);

        policies.Should().NotContain(p => p.PolicyType == typeof(NonCQRSOCP));
    }

    [Fact]
    public void EnumeratePolicies_ignores_cqrs_policies_referencing_non_query_nor_operation_in_assemblies()
    {
        var policies = CQRSOutputCacheRegistry.EnumeratePolicies([typeof(TestQueryOCP).Assembly]);

        policies.Should().NotContain(p => p.PolicyType == typeof(RandomObjectOCP));
    }

    [Fact]
    public void Register_throws_when_contract_type_is_not_query_nor_operation()
    {
        var registry = new CQRSOutputCacheRegistry();

        var act = () => registry.Register(typeof(TestCommand), typeof(TestQueryOCP));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Register_throws_when_policy_type_is_not_output_cache_policy()
    {
        var registry = new CQRSOutputCacheRegistry();

        var act = () => registry.Register(typeof(TestQuery), typeof(InvalidOCP));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Register_throws_when_multiple_policies_are_registered_for_the_same_contract()
    {
        var registry = new CQRSOutputCacheRegistry();
        registry.Register(typeof(TestQuery), typeof(TestQueryOCP));

        var act = () => registry.Register(typeof(TestQuery), typeof(TestQueryOCP));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetAdditionalEndpointMetadata_returns_output_cache_attribute_when_policy_is_registered()
    {
        var registry = new CQRSOutputCacheRegistry();
        registry.Register(typeof(TestQuery), typeof(TestQueryOCP));

        var metadata = CreateMetadata(typeof(TestQuery));

        var additionalMetadata = registry.GetAdditionalEndpointMetadata(metadata).ToList();

        additionalMetadata
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<OutputCacheAttribute>()
            .Subject.PolicyName.Should()
            .Be(CQRSOutputCachePolicyName.For(typeof(TestQueryOCP)));
    }

    [Fact]
    public void GetAdditionalEndpointMetadata_returns_empty_when_policy_is_not_registered()
    {
        var registry = new CQRSOutputCacheRegistry();
        var metadata = CreateMetadata(typeof(TestQuery));

        var additionalMetadata = registry.GetAdditionalEndpointMetadata(metadata);

        additionalMetadata.Should().BeEmpty();
    }

    private static CQRSObjectMetadata CreateMetadata(Type objectType)
    {
        return new(CQRSObjectKind.Query, objectType, typeof(TestResult), typeof(TestHandler), Executor);
    }
}
