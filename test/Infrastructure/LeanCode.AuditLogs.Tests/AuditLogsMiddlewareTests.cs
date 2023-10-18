using MassTransit;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.AuditLogs.Tests;

public sealed class AuditLogsMiddlewareTests
{
    [Fact]
    public async void Extracts_changes_after_pipeline_execution()
    {
        var requestPath = "/test.request.path";
        var dbContext = new TestDbContext();
        var bus = Substitute.For<IBus>();
        var publisher = Substitute.For<AuditLogsPublisher>();
        var requestDelegate = Substitute.For<RequestDelegate>();
        var middleware = new AuditLogsMiddleware<TestDbContext>(requestDelegate);

        var httpContext = Substitute.For<HttpContext>();
        httpContext.Request.Path.Returns(PathString.FromUriComponent(requestPath));
        await middleware.InvokeAsync(httpContext, dbContext, bus, publisher);

        await publisher.Received().ExtractAndPublishAsync(dbContext, bus, requestPath, Arg.Any<CancellationToken>());
    }
}
