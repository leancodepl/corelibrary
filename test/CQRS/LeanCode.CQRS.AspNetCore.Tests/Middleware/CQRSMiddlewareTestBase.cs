using System.Diagnostics.Metrics;
using FluentAssertions;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

public abstract class CQRSMiddlewareTestBase<TMiddleware> : IAsyncLifetime
{
    private readonly MetricCollector<int> cqrsSuccessMetricCollector;
    private readonly MetricCollector<int> cqrsFailureMetricCollector;
    protected IHost Host { get; }
    protected TestServer Server { get; }

    protected RequestDelegate FinalPipeline { get; set; } = ctx => Task.CompletedTask;

    protected virtual void ConfigureServices(IServiceCollection services) { }

    protected CQRSMiddlewareTestBase()
    {
        Host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<CQRSMetrics>();
                        ConfigureServices(services);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<TMiddleware>();
                        app.Run(ctx => FinalPipeline(ctx));
                    });
            })
            .Build();

        Server = Host.GetTestServer();

        var meterFactory = Host.Services.GetRequiredService<IMeterFactory>();

        cqrsSuccessMetricCollector = new MetricCollector<int>(meterFactory, LeanCodeMetrics.MeterName, "cqrs.success");
        cqrsFailureMetricCollector = new MetricCollector<int>(meterFactory, LeanCodeMetrics.MeterName, "cqrs.failure");
    }

    protected void VerifyCQRSSuccessMetrics(int measuredTotal)
    {
        var snapshot = cqrsSuccessMetricCollector.GetMeasurementSnapshot();
        var metric = snapshot.Sum(s => s.Value);
        metric.Should().Be(measuredTotal, "cqrs success metric should be equal {0}", measuredTotal);

        VerifyNoCQRSFailureMetrics();
    }

    protected void VerifyNoCQRSFailureMetrics()
    {
        var snapshot = cqrsFailureMetricCollector.GetMeasurementSnapshot();
        var metric = snapshot.Sum(m => m.Value);
        metric.Should().Be(0, "there should be no error metrics");
    }

    protected void VerifyCQRSFailureMetrics(string reason, int measuredTotal)
    {
        var snapshot = cqrsFailureMetricCollector.GetMeasurementSnapshot();

        var metric = snapshot
            .Where(m => m.MatchesTags(KeyValuePair.Create<string, object?>("reason", reason)))
            .Sum(m => m.Value);

        metric.Should().Be(measuredTotal, "collected `{0}` metric should be equal to {1}", reason, measuredTotal);
        VerifyNoCQRSSuccessMetrics();
    }

    protected void VerifyNoCQRSSuccessMetrics()
    {
        var snapshot = cqrsSuccessMetricCollector.GetMeasurementSnapshot();
        var metric = snapshot.Sum(m => m.Value);
        metric.Should().Be(0, "there should be no success metrics");
    }

    public Task InitializeAsync() => Host.StartAsync();

    public Task DisposeAsync() => Host.StopAsync();
}
