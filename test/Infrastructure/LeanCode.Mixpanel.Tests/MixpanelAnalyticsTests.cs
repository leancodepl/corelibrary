using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.Mixpanel.Tests
{
    public class MixpanelAnalyticsTests
    {
        private static readonly MixpanelConfiguration Configuration = new MixpanelConfiguration
        {
            ApiKey = Environment.GetEnvironmentVariable("MIXPANEL_APIKEY"),
            Token = Environment.GetEnvironmentVariable("MIXPANEL_TOKEN"),
        };

        private readonly MixpanelAnalytics analytics;

        public MixpanelAnalyticsTests()
        {
            analytics = new MixpanelAnalytics(
                new HttpClient
                {
                    BaseAddress = new Uri("https://api.mixpanel.com"),
                },
                Configuration);
        }

        [MixpanelFact]
        public async Task Track_works()
        {
            await analytics.TrackAsync(Guid.NewGuid().ToString(), "ActivityCreated", "activityId", Guid.NewGuid().ToString());
        }

        public class MixpanelFactAttribute : FactAttribute
        {
            public MixpanelFactAttribute()
            {
                if (string.IsNullOrEmpty(Configuration.ApiKey))
                {
                    Skip = "API key not set";
                }
                else if (string.IsNullOrEmpty(Configuration.Token))
                {
                    Skip = "Token not set";
                }
            }
        }
    }
}
