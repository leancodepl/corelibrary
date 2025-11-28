using System.Runtime.CompilerServices;
using LeanCode.Logging;
using LeanCode.Test.Helpers;

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
        analytics = new MixpanelAnalytics(
            new HttpClient { BaseAddress = new Uri("https://api.mixpanel.com") },
            Configuration,
            NullLogger<MixpanelAnalytics>.Instance
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

internal sealed class MixpanelFactAttribute(
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = 0
) : ExternalServiceFactAttribute(sourceFilePath, sourceLineNumber)
{
    protected override string ServiceType => "mixpanel";
    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; } =
    ["MIXPANEL_APIKEY", "MIXPANEL_TOKEN"];
}
