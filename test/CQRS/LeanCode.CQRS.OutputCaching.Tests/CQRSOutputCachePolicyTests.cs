using FluentAssertions;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.BasePolicies;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LeanCode.CQRS.OutputCaching.Tests;

public class CQRSOutputCachePolicyTests
{
    [Fact]
    public void GetPayload_returns_payload_set_on_http_context()
    {
        var context = new DefaultHttpContext();
        var query = new TestQuery();
        context.SetCQRSRequestPayload(query);

        var policy = new TestQueryOCP();

        policy.GetRequestPayload(context).Should().BeSameAs(query);
    }

    [Fact]
    public void GetResult_returns_query_result_from_execution_result()
    {
        var context = new DefaultHttpContext();
        var result = new TestResult();
        context.Features.Set(ExecutionResult.WithPayload(result));

        var cacheResult = CQRSQueryOutputCachePolicy<TestQuery, TestResult>.GetResultPayload(context);

        cacheResult.Should().BeSameAs(result);
    }
}
