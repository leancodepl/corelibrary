using System.Collections.Generic;

namespace LeanCode.Mixpanel
{
    public interface IMixpanelEvent
    {
        string EventName { get; }
        Dictionary<string, object> Properties { get; }
    }
}
