using FluentAssertions;
using FluentAssertions.Execution;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching;
using LeanCode.CQRS.OutputCaching.BasePolicies;
using LeanCode.CQRS.OutputCaching.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

public class CQRSOutputCachingObservabilityMiddlewareTests : CQRSMiddlewareTestBase
{
    private static readonly CQRSObjectMetadata TestMetadata = new(
        CQRSObjectKind.Query,
        typeof(TestQuery),
        typeof(TestQueryResult),
        typeof(TestQueryHandler),
        (_, _) => Task.FromResult<object?>(null)
    );

    private static readonly string TestPolicyName = CQRSOutputCachePolicyName.For(typeof(TestQueryOCP));

    private readonly MetricCollector<int> cqrsCacheHitMetricCollector;
    private readonly MetricCollector<int> cqrsCacheMissMetricCollector;

    public CQRSOutputCachingObservabilityMiddlewareTests()
        : base(app => app.UseMiddleware<CQRSOutputCachingObservabilityMiddleware>().UseOutputCache())
    {
        cqrsCacheHitMetricCollector = new(CQRSOutputCacheMetrics.CqrsOutputCacheHit);
        cqrsCacheMissMetricCollector = new(CQRSOutputCacheMetrics.CqrsOutputCacheMiss);

        FinalPipeline = ctx => ctx.CompleteCQRSExecutionResult(ExecutionResult.WithPayload(new TestQueryResult(42)));
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddCQRSOutputCache(TypesCatalog.Of<TestQueryOCP>());
    }

    [Fact]
    public async Task Records_span_without_policy_when_output_cache_metadata_is_missing()
    {
        await SendRequestAsync(includeOutputCacheMetadata: false);

        using (new AssertionScope())
        {
            VerifyNoCQRSCacheHitMetrics();
            VerifyNoCQRSCacheMissMetrics();
            VerifyOutputCacheActivity(
                servedFromCache: null,
                policyPresent: false,
                storedInCache: null,
                allowCacheStorage: null,
                allowCacheLookup: null,
                allowLocking: null
            );
        }
    }

    [Fact]
    public async Task Records_cache_misses_and_cache_hits()
    {
        await SendRequestAsync(query: new());

        using (new AssertionScope())
        {
            VerifyCQRSCacheMissMetrics(1);
            VerifyNoCQRSCacheHitMetrics();
            VerifyOutputCacheActivity(servedFromCache: false, storedInCache: true);
        }

        await SendRequestAsync(query: new());

        using (new AssertionScope())
        {
            VerifyCQRSCacheMissMetrics(1);
            VerifyCQRSCacheHitMetrics(1);
            VerifyOutputCacheActivity(servedFromCache: true);
        }
    }

    [Fact]
    public async Task Does_not_collect_metrics_when_output_cache_disabled()
    {
        await SendRequestAsync(query: new() { EnableOutputCaching = false });

        using (new AssertionScope())
        {
            VerifyNoCQRSCacheHitMetrics();
            VerifyNoCQRSCacheMissMetrics();
            VerifyOutputCacheActivity(
                servedFromCache: null,
                policyPresent: null,
                storedInCache: null,
                allowCacheStorage: null,
                allowCacheLookup: null,
                allowLocking: null
            );
        }
    }

    [Fact]
    public async Task Records_activity_tags_when_cache_storage_is_not_allowed_after_execution()
    {
        await SendRequestAsync(query: new() { AllowCacheStoreAfterExecution = false });

        using (new AssertionScope())
        {
            VerifyCQRSCacheMissMetrics(1);
            VerifyNoCQRSCacheHitMetrics();
            VerifyOutputCacheActivity(servedFromCache: false, storedInCache: false, allowCacheStorage: false);
        }
    }

    [Fact]
    public async Task Records_activity_tags_when_cache_lookup_is_not_allowed()
    {
        await SendRequestAsync(query: new() { LookUpInCache = false });

        using (new AssertionScope())
        {
            VerifyCQRSCacheMissMetrics(1);
            VerifyNoCQRSCacheHitMetrics();
            VerifyOutputCacheActivity(servedFromCache: false, storedInCache: true, allowCacheLookup: false);
        }
    }

    [Fact]
    public async Task Records_activity_tags_when_cache_lookup_is_not_allowed_nor_cache_storage_before_execution()
    {
        await SendRequestAsync(query: new() { LookUpInCache = false, AllowCacheStoreBeforeExecution = false });

        using (new AssertionScope())
        {
            VerifyCQRSCacheMissMetrics(1);
            VerifyNoCQRSCacheHitMetrics();
            VerifyOutputCacheActivity(
                servedFromCache: false,
                storedInCache: false,
                allowCacheStorage: false,
                allowCacheLookup: false
            );
        }
    }

    [Fact]
    public async Task Records_activity_tags_when_locking_is_not_allowed()
    {
        await SendRequestAsync(query: new() { AllowLocking = false });

        using (new AssertionScope())
        {
            VerifyCQRSCacheMissMetrics(1);
            VerifyNoCQRSCacheHitMetrics();
            VerifyOutputCacheActivity(servedFromCache: false, storedInCache: true, allowLocking: false);
        }
    }

    private void VerifyCQRSCacheHitMetrics(int measuredTotal)
    {
        var snapshot = cqrsCacheHitMetricCollector.GetMeasurementSnapshot();
        var metric = snapshot.Sum(s => s.Value);
        metric.Should().Be(measuredTotal, "collected cache hit metric should be equal {0}", measuredTotal);

        VerifyNoCQRSFailureMetrics();
    }

    private void VerifyNoCQRSCacheMissMetrics()
    {
        var snapshot = cqrsCacheMissMetricCollector.GetMeasurementSnapshot();
        var metric = snapshot.Sum(m => m.Value);
        metric.Should().Be(0, "there should be no cache miss metrics");
    }

    private void VerifyCQRSCacheMissMetrics(int measuredTotal)
    {
        var snapshot = cqrsCacheMissMetricCollector.GetMeasurementSnapshot();
        var metric = snapshot.Sum(m => m.Value);

        metric.Should().Be(measuredTotal, "collected cache miss metric should be equal to {0}", measuredTotal);
        VerifyNoCQRSSuccessMetrics();
    }

    private void VerifyNoCQRSCacheHitMetrics()
    {
        var snapshot = cqrsCacheHitMetricCollector.GetMeasurementSnapshot();
        var metric = snapshot.Sum(m => m.Value);
        metric.Should().Be(0, "there should be no cache hit metrics");
    }

    private Task<HttpContext> SendRequestAsync(bool includeOutputCacheMetadata = true, TestQuery? query = null)
    {
        var configuredQuery = query ?? new TestQuery();

        return Server.SendAsync(ctx =>
        {
            ctx.Request.Method = HttpMethods.Post;
            ctx.SetCQRSRequestPayload(configuredQuery);

            object[] additionalMetadata = includeOutputCacheMetadata
                ? [new OutputCacheAttribute { PolicyName = TestPolicyName }]
                : [];

            ctx.SetEndpoint(TestHelpers.MockCQRSEndpoint(TestMetadata, additionalMetadata));
        });
    }

    private void VerifyOutputCacheActivity(
        bool? servedFromCache,
        bool? policyPresent = null,
        bool? storedInCache = null,
        bool? allowCacheStorage = true,
        bool? allowCacheLookup = true,
        bool? allowLocking = true
    )
    {
        var dict = new Dictionary<string, object?>
        {
            { "output_cache.locking_allowed", allowLocking },
            { "output_cache.storage_allowed", allowCacheStorage },
            { "output_cache.lookup_allowed", allowCacheLookup },
            { "output_cache.served_from_cache", servedFromCache },
            { "output_cache.stored_in_cache", storedInCache },
        };

        if (policyPresent is false)
        {
            dict["output_cache.policy"] = null;
            dict["output_cache.policy_present"] = false;
        }
        else
        {
            dict["output_cache.policy"] = typeof(TestQueryOCP).FullName;
        }

        VerifyActivity("middleware - OutputCache", additionalTags: dict);
    }

    private sealed record TestQuery : IQuery<TestQueryResult>
    {
        public bool EnableOutputCaching { get; init; } = true;
        public bool LookUpInCache { get; init; } = true;
        public bool AllowCacheStoreBeforeExecution { get; init; } = true;
        public bool AllowCacheStoreAfterExecution { get; init; } = true;
        public bool AllowLocking { get; init; } = true;
    }

    private sealed record TestQueryResult(int Value);

    private sealed class TestQueryHandler;

    private sealed class TestQueryOCP : CQRSOutputCachePolicy<TestQuery>
    {
        public override ValueTask CacheRequestCoreAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var query = context.HttpContext.GetCQRSRequestPayload<TestQuery>();
            context.EnableOutputCaching = query.EnableOutputCaching;
            context.AllowCacheLookup = query.LookUpInCache;
            context.AllowCacheStorage = query.AllowCacheStoreBeforeExecution;
            context.AllowLocking = query.AllowLocking;
            return ValueTask.CompletedTask;
        }

        public override async ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var query = context.HttpContext.GetCQRSRequestPayload<TestQuery>();
            context.AllowCacheStorage = query.AllowCacheStoreAfterExecution;
            await base.ServeResponseAsync(context, cancellation);
        }
    }
}
