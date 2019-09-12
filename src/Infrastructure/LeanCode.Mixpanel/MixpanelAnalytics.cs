using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeanCode.Mixpanel
{
    [SuppressMessage(
        "StyleCop.CSharp.LayoutRules",
        "SA1507:CodeMustNotContainMultipleBlankLinesInARow",
        Justification = "Reviewed.")]
    public class MixpanelAnalytics
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<MixpanelAnalytics>();

        private readonly MixpanelHttpClient client;
        private readonly MixpanelConfiguration configuration;

        public MixpanelAnalytics(MixpanelHttpClient client, MixpanelConfiguration configuration)
        {
            this.configuration = configuration;
            this.client = client;
        }

        public Task Alias(string newId, string oldId)
        {
            return Track(newId, "$create_alias", new Dictionary<string, object>()
            {
                ["distinct_id"] = oldId,
                ["alias"] = newId,
            });
        }

        public Task Track(string userId, IMixpanelEvent mixpanelEvent, bool isImport = false)
            => Track(userId, mixpanelEvent.EventName, mixpanelEvent.Properties, isImport);

        public Task Track(string userId, string eventName, string propertyName, string propertyValue, bool isImport = false)
            => Track(userId, eventName, new Dictionary<string, object>() { [propertyName] = propertyValue }, isImport);

        public Task Track(string userId, string eventName, Dictionary<string, object> properties, bool isImport = false)
            => TrackEvent(userId, eventName, properties, isImport);


        public Task Set(string userId, string name, string value)
            => Set(userId, new Dictionary<string, string>() { [name] = value });

        public Task Set(string userId, Dictionary<string, string> properties)
            => Engage(userId, "$set", properties);


        public Task Add(string userId, string name, string value)
            => Add(userId, new Dictionary<string, string>() { [name] = value });

        public Task Add(string userId, Dictionary<string, string> properties)
            => Engage(userId, "$add", properties);


        public Task Append(string userId, string name, object value)
            => Append(userId, new Dictionary<string, object>() { [name] = value });

        public Task Append(string userId, Dictionary<string, object> properties)
            => Engage(userId, "$append", properties);


        public Task SetOnce(string userId, string name, string value)
            => SetOnce(userId, new Dictionary<string, string>() { [name] = value });

        public Task SetOnce(string userId, Dictionary<string, string> properties)
            => Engage(userId, "$set_once", properties);


        public Task Union(string userId, string name, List<string> elements)
            => Union(userId, new Dictionary<string, List<string>>() { [name] = elements });

        public Task Union(string userId, Dictionary<string, List<string>> properties)
            => Engage(userId, "$union", properties);


        public Task Unset(string userId, string property)
            => Unset(userId, new List<string>() { property });

        public Task Unset(string userId, List<string> properties)
            => Engage(userId, "$unset", properties);


        public Task Delete(string userId)
            => Engage(userId, "$delete");


        private Task TrackEvent(string userId, string name, Dictionary<string, object>? properties, bool isImport)
        {
            properties ??= new Dictionary<string, object>();

            properties["token"] = configuration.Token;

            if (!properties.ContainsKey("distinct_id"))
            {
                properties["distinct_id"] = userId;
            }

            var data = new Dictionary<string, object?>()
            {
                ["event"] = name,
                ["properties"] = properties,
            };

            logger.Verbose("Sending Mixpanel event {eventName} for user {userId}", name, userId);

            return MakeRequest(userId, isImport ? "import" : "track", name, data);
        }

        private Task Engage(string userId, string operation, object? properties = null)
        {
            logger.Verbose("Engaging Mixpanel operation {name} for user {userId}", operation, userId);
            var data = new Dictionary<string, object?>()
            {
                ["$token"] = configuration.Token,
                ["$distinct_id"] = userId,
                [operation] = properties,
            };

            return MakeRequest(userId, "engage", operation, data);
        }

        private async Task MakeRequest(string userId, string uri, string requestName, Dictionary<string, object?> data)
        {
            string dataString = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));

            var jsonResponse = await client.Client
                .GetStringAsync($"{uri}/?data={dataString}&verbose=1&api_key={configuration.ApiKey}")
                .ConfigureAwait(false);

            var response = JsonConvert.DeserializeObject<MixpanelResponse>(jsonResponse);

            if (response.Status == MixpanelResponse.Success)
            {
                logger.Information(
                    "Mixpanel request {requestName} for user {userId} sent successfully",
                    requestName, userId);
            }
            else
            {
                logger.Warning(
                    "Error sending mixpanel request {requestName} for user {userId} with data: {@EventData}",
                    requestName, userId, data);

                logger.Warning(
                    "Mixpanel returned error: {Error}",
                    response.Error);
            }
        }
    }

    internal class MixpanelResponse
    {
        public const int Success = 1;
        public const int Failure = 0;

        public int Status { get; set; }
        public string? Error { get; set; }
    }
}
