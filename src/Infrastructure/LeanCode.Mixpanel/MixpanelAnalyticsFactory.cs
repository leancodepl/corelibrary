using System.Net.Http;

namespace LeanCode.Mixpanel
{
    class MixpanelAnalyticsFactory : IMixpanelAnalyticsFactory
    {
        private HttpClient sharedClient;
        public MixpanelAnalyticsFactory()
        {
            sharedClient = new HttpClient();
        }
        public IMixpanelAnalytics Create(MixpanelConfiguration configuration)
        {
            return new MixpanelAnalytics(sharedClient, configuration);
        }
    }
}
