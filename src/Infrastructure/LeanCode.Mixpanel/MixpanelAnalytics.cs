using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

            logger.Verbose("Sending Mixpanel event {EventName} for user {UserId}", name, userId);

            return MakeRequest(userId, isImport ? "import" : "track", name, data);
        }

        private Task Engage(string userId, string operation, object? properties = null)
        {
            logger.Verbose("Engaging Mixpanel operation {Name} for user {UserId}", operation, userId);
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
            var dataString = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(data));
            var url = $"{uri}/?data={dataString}&verbose=1&api_key={configuration.ApiKey}";

            using var rawResponse = await client.Client.GetAsync(url);
            var content = await rawResponse.Content.ReadAsStringAsync();
            if (content == "1")
            {
                logger.Debug(
                    "Mixpanel request {RequestName} for user {UserId} sent successfully",
                    requestName, userId);
            }
            else if (content == "0")
            {
                logger.Warning(
                    "Error sending mixpanel request {RequestName} for user {UserId} with data: {@EventData}",
                    requestName, userId, data);
            }
            else
            {
                var response = JsonSerializer.Deserialize<MixpanelResponse>(content);

                if (response.Status == MixpanelResponse.Success)
                {
                    logger.Information(
                        "Mixpanel request {RequestName} for user {UserId} sent successfully",
                        requestName, userId);
                }
                else
                {
                    logger.Warning(
                        "Error sending mixpanel request {RequestName} for user {UserId} with data: {@EventData}. Mixpanel returned an error {Error}",
                        requestName, userId, data);
                }
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
