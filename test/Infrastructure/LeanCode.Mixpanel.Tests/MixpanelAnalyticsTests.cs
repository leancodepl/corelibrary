using LeanCode.Test.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LeanCode.Mixpanel.Tests;

public class MixpanelAnalyticsTests
{
    private static readonly MixpanelConfiguration Configuration = new MixpanelConfiguration
    {
        ApiKey = Environment.GetEnvironmentVariable("MIXPANEL_APIKEY"),
        Token = Environment.GetEnvironmentVariable("MIXPANEL_TOKEN"),
    };

    private readonly MixpanelAnalytics analytics;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA2000",
        Justification = "References don't go out of scope."
    )]
    public MixpanelAnalyticsTests()
    {
        var logger = Substitute.For<ILogger<MixpanelAnalytics>>();

        analytics = new MixpanelAnalytics(
            logger,
            new HttpClient { BaseAddress = new Uri("https://api.mixpanel.com") },
            Configuration
        );
    }

    [MixpanelFact]
    public async Task Track_works()
    {
        await analytics.TrackAsync(
            Guid.NewGuid().ToString(),
            "ActivityCreated",
            "activityId",
            Guid.NewGuid().ToString()
        );
    }
}

internal sealed class MixpanelFactAttribute : ExternalServiceFactAttribute
{
    protected override string ServiceType => "mixpanel";
    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; } =
        ["MIXPANEL_APIKEY", "MIXPANEL_TOKEN"];
}
