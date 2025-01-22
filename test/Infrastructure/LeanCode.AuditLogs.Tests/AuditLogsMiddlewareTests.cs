using MassTransit;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.AuditLogs.Tests;

public sealed class AuditLogsMiddlewareTests
{
    [Fact]
    public async Task Extracts_changes_after_pipeline_execution()
    {
        const string RequestPath = "/test.request.path";
        using var dbContext = new TestDbContext();
        var bus = Substitute.For<IBus>();
        var publisher = Substitute.For<AuditLogsPublisher>();
        var middleware = new AuditLogsMiddleware<TestDbContext>(_ => Task.CompletedTask);
        var httpContext = Substitute.For<HttpContext>();
        httpContext.Request.Path.Returns(PathString.FromUriComponent(new Uri(RequestPath)));

        await middleware.InvokeAsync(httpContext, dbContext, bus, publisher);

        await publisher.Received().ExtractAndPublishAsync(dbContext, bus, RequestPath, Arg.Any<CancellationToken>());
    }
}
