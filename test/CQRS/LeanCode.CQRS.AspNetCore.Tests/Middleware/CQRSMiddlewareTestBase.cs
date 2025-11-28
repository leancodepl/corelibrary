using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Serialization;
using LeanCode.Logging.AspNetCore;
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

public abstract class CQRSMiddlewareTestBase<TMiddleware>()
    : CQRSMiddlewareTestBase(app => app.UseMiddleware<TMiddleware>());

[SuppressMessage("?", "CA1063", Justification = "Demands a Dispose(bool) pattern which is not applicable")]
public abstract class CQRSMiddlewareTestBase : IAsyncLifetime, IDisposable
{
    private readonly ConcurrentBag<Activity> activities;
    private readonly ActivityListener activityListener;
    private readonly MetricCollector<int> cqrsSuccessMetricCollector;
    private readonly MetricCollector<int> cqrsFailureMetricCollector;
    protected SerializerMock Serializer { get; }
    protected IHost Host { get; }
    protected TestServer Server { get; }

    protected RequestDelegate FinalPipeline { get; set; } = _ => Task.CompletedTask;

    protected virtual void ConfigureServices(IServiceCollection services) { }

    protected CQRSMiddlewareTestBase(Action<IApplicationBuilder> configureMiddleware)
    {
        Serializer = new();
        Host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<CQRSMetrics>();
                        services.AddSingleton<ISerializer>(Serializer);
                        ConfigureServices(services);
                    })
                    .Configure(app =>
                    {
                        configureMiddleware(app);
                        app.Run(ctx => FinalPipeline(ctx));
                    });
            })
            .ConfigureDefaultLogging("test")
            .Build();

        Server = Host.GetTestServer();

        activities = [];
        activityListener = new()
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = a => activities.Add(a),
        };
        ActivitySource.AddActivityListener(activityListener);

        var meterFactory = Host.Services.GetRequiredService<IMeterFactory>();
        cqrsSuccessMetricCollector = new(meterFactory, LeanCodeMetrics.MeterName, "cqrs.success");
        cqrsFailureMetricCollector = new(meterFactory, LeanCodeMetrics.MeterName, "cqrs.failure");
    }

    protected void VerifyActivity(
        string operationNamePrefix,
        ActivityStatusCode? activityStatusCode = null,
        string? failureReason = null,
        bool exceptionShouldBeRecorded = false,
        IReadOnlyDictionary<string, object?>? additionalTags = null
    )
    {
        var activity = activities
            .Where(a => a.OperationName.StartsWith(operationNamePrefix, StringComparison.Ordinal))
            .MaxBy(a => a.StartTimeUtc);

        activity.Should().NotBeNull("there should be some activities for middleware execution");

        if (activityStatusCode is not null)
        {
            activity.Status.Should().Be(activityStatusCode.Value);
        }

        if (failureReason is not null)
        {
            activity.GetTagItem(CQRSMetrics.FailureReasonKey).Should().Be(failureReason);
        }

        if (exceptionShouldBeRecorded)
        {
            activity.Events.Should().Contain(e => e.Name == "exception");
        }

        if (additionalTags is not null)
        {
            var tagsLookup = additionalTags.ToLookup(t => t.Value is not null);
            if (tagsLookup[true].Any())
            {
                activity.TagObjects.Should().Contain(tagsLookup[true]);
            }

            if (tagsLookup[false].Any())
            {
                activity.TagObjects.Should().NotContainKeys(tagsLookup[false].Select(t => t.Key));
            }
        }
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
            .Where(m => m.MatchesTags(KeyValuePair.Create<string, object?>(CQRSMetrics.FailureReasonKey, reason)))
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

    public async ValueTask InitializeAsync() => await Host.StartAsync();

    public async ValueTask DisposeAsync() => await Host.StopAsync();

    public void Dispose()
    {
        Server.Dispose();
        Host.Dispose();

        activityListener.Dispose();
        cqrsSuccessMetricCollector.Dispose();
        cqrsFailureMetricCollector.Dispose();
    }
}
