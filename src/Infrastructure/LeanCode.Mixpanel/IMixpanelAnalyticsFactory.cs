namespace LeanCode.Mixpanel
{
    public interface IMixpanelAnalyticsFactory
    {
        IMixpanelAnalytics Create(MixpanelConfiguration configuration);
    }
}
