using System;
using System.Net.Http;

namespace LeanCode.Mixpanel
{
    class MixpanelAnalyticsFactory : IMixpanelAnalyticsFactory, IDisposable
    {
        private readonly HttpClient sharedClient;

        public MixpanelAnalyticsFactory()
        {
            sharedClient = new HttpClient();
        }

        public IMixpanelAnalytics Create(MixpanelConfiguration configuration)
        {
            return new MixpanelAnalytics(sharedClient, configuration);
        }

        public void Dispose()
        {
            sharedClient.Dispose();
        }
    }
}
