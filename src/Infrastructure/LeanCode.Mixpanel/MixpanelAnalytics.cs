using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        public Task AliasAsync(string newId, string oldId)
        {
            return TrackAsync(newId, "$create_alias", new Dictionary<string, object>()
            {
                ["distinct_id"] = oldId,
                ["alias"] = newId,
            });
        }

        public Task TrackAsync(string userId, IMixpanelEvent mixpanelEvent, bool isImport = false)
            => TrackAsync(userId, mixpanelEvent.EventName, mixpanelEvent.Properties, isImport);

        public Task TrackAsync(string userId, string eventName, string propertyName, string propertyValue, bool isImport = false)
            => TrackAsync(userId, eventName, new Dictionary<string, object>() { [propertyName] = propertyValue }, isImport);

        public Task TrackAsync(string userId, string eventName, Dictionary<string, object> properties, bool isImport = false)
            => TrackEventAsync(userId, eventName, properties, isImport);


        public Task SetAsync(string userId, string name, string value)
            => SetAsync(userId, new Dictionary<string, string>() { [name] = value });

        public Task SetAsync(string userId, Dictionary<string, string> properties)
            => EngageAsync(userId, "$set", properties);


        public Task AddAsync(string userId, string name, string value)
            => AddAsync(userId, new Dictionary<string, string>() { [name] = value });

        public Task AddAsync(string userId, Dictionary<string, string> properties)
            => EngageAsync(userId, "$add", properties);


        public Task AppendAsync(string userId, string name, object value)
            => AppendAsync(userId, new Dictionary<string, object>() { [name] = value });

        public Task AppendAsync(string userId, Dictionary<string, object> properties)
            => EngageAsync(userId, "$append", properties);


        public Task SetOnceAsync(string userId, string name, string value)
            => SetOnceAsync(userId, new Dictionary<string, string>() { [name] = value });

        public Task SetOnceAsync(string userId, Dictionary<string, string> properties)
            => EngageAsync(userId, "$set_once", properties);


        public Task UnionAsync(string userId, string name, List<string> elements)
            => UnionAsync(userId, new Dictionary<string, List<string>>() { [name] = elements });

        public Task UnionAsync(string userId, Dictionary<string, List<string>> properties)
            => EngageAsync(userId, "$union", properties);


        public Task UnsetAsync(string userId, string property)
            => UnsetAsync(userId, new List<string>() { property });

        public Task UnsetAsync(string userId, List<string> properties)
            => EngageAsync(userId, "$unset", properties);


        public Task DeleteAsync(string userId)
            => EngageAsync(userId, "$delete");


        private Task TrackEventAsync(string userId, string name, Dictionary<string, object>? properties, bool isImport)
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

            return MakeRequestAsync(userId, isImport ? "import" : "track", name, data);
        }

        private Task EngageAsync(string userId, string operation, object? properties = null)
        {
            logger.Verbose("Engaging Mixpanel operation {Name} for user {UserId}", operation, userId);
            var data = new Dictionary<string, object?>()
            {
                ["$token"] = configuration.Token,
                ["$distinct_id"] = userId,
                [operation] = properties,
            };

            return MakeRequestAsync(userId, "engage", operation, data);
        }

        private async Task MakeRequestAsync(string userId, string uri, string requestName, Dictionary<string, object?> data)
        {
            var dataString = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(data));
            var url = $"{uri}/?data={dataString}&verbose={(configuration.VerboseErrors ? "1" : "0")}&api_key={configuration.ApiKey}";

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

        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
