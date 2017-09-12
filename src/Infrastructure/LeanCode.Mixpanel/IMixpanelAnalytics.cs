using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCode.Mixpanel
{
    public interface IMixpanelAnalytics
    {
        Task Alias(string newId, string oldId);
        Task Track(string userId, IMixpanelEvent mixpanelEvent, bool isImport = false);
        Task Track(string userId, string eventName, string propertyName, string propertyValue, bool isImport = false);
        Task Track(string userId, string eventName, Dictionary<string, object> properties, bool isImport = false);
        Task Set(string userId, string name, string value);
        Task Set(string userId, Dictionary<string, string> properties);
        Task Add(string userId, string name, string value);
        Task Add(string userId, Dictionary<string, string> properties);
        Task Append(string userId, Dictionary<string, object> properties);
        Task SetOnce(string userId, string name, string value);
        Task SetOnce(string userId, Dictionary<string, string> properties);
        Task Union(string userId, string name, List<string> elements);
        Task Union(string userId, Dictionary<string, List<string>> properties);
        Task Unset(string userId, string property);
        Task Unset(string userId, List<string> properties);
        Task Delete(string userId);
    }
}
