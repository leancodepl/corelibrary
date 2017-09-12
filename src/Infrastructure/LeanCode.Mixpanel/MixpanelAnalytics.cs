using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LeanCode.Mixpanel
{
    class MixpanelAnalytics : IMixpanelAnalytics, IDisposable
    {
        private readonly HttpClient client;
        private readonly MixpanelConfiguration mixpanelConfiguration;

        public MixpanelAnalytics(
            IOptions<MixpanelConfiguration> mixpanelConfiguration)
        {
            this.mixpanelConfiguration = mixpanelConfiguration.Value;
            this.client = new HttpClient();
        }

        public Task Alias(string newId, string oldId)
        {
            return Track(newId, "$create_alias", new Dictionary<string, object>()
            {
                ["distinct_id"] = oldId,
                ["alias"] = newId
            });
        }

        public Task Track(string userId, IMixpanelEvent mixpanelEvent, bool isImport = false)
            => Track(userId, mixpanelEvent.EventName, mixpanelEvent.Properties, isImport);

        public Task Track(string userId, string eventName, string propertyName, string propertyValue, bool isImport = false)
            => Track(userId, eventName, new Dictionary<string, object>() {[propertyName] = propertyValue}, isImport);

        public Task Track(string userId, string eventName, Dictionary<string, object> properties, bool isImport = false)
            => TrackEvent(userId, eventName, properties, isImport);


        public Task Set(string userId, string name, string value)
            => Set(userId, new Dictionary<string, string>() {[name] = value});

        public Task Set(string userId, Dictionary<string, string> properties)
            => Engage(userId, "$set", properties);


        public Task Add(string userId, string name, string value)
            => Add(userId, new Dictionary<string, string>() {[name] = value});

        public Task Add(string userId, Dictionary<string, string> properties)
            => Engage(userId, "$add", properties);


        public Task Append(string userId, string name, object value)
            => Append(userId, new Dictionary<string, object>() {[name] = value });

        public Task Append(string userId, Dictionary<string, object> properties)
            => Engage(userId, "$append", properties);


        public Task SetOnce(string userId, string name, string value)
            => SetOnce(userId, new Dictionary<string, string>() {[name] = value});

        public Task SetOnce(string userId, Dictionary<string, string> properties)
            => Engage(userId, "$set_once", properties);


        public Task Union(string userId, string name, List<string> elements)
            => Union(userId, new Dictionary<string, List<string>>() {[name] = elements});

        public Task Union(string userId, Dictionary<string, List<string>> properties)
            => Engage(userId, "$union", properties);


        public Task Unset(string userId, string property)
            => Unset(userId, new List<string>() {property});

        public Task Unset(string userId, List<string> properties)
            => Engage(userId, "$unset", properties);


        public Task Delete(string userId)
            => Engage(userId, "$delete");


        private Task TrackEvent(string userId, string name, Dictionary<string, object> properties, bool isImport)
        {
            var data = new Dictionary<string, object>();
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }

            properties["token"] = mixpanelConfiguration.Token;
            if (!properties.ContainsKey("distinct_id"))
            {
                properties["distinct_id"] = userId;
            }

            data["event"] = name;
            data["properties"] = properties;

            return MakeRequest(isImport ? "import" : "track", data);
        }

        private Task Engage(string userId, string operation, object properties = null)
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["$token"] = mixpanelConfiguration.Token,
                ["$distinct_id"] = userId,
                [operation] = properties
            };

            return MakeRequest("engage", data);
        }

        private Task MakeRequest(string uri, Dictionary<string, object> data)
        {
            string dataString = JsonConvert.SerializeObject(data);
            dataString = Convert.ToBase64String(Encoding.UTF8.GetBytes(dataString));

            return client.GetAsync($"http://api.mixpanel.com/{uri}/?data={dataString}&verbose=1&api_key={mixpanelConfiguration.ApiKey}");
        }

        void IDisposable.Dispose()
        {
            client.Dispose();
        }
    }
}
