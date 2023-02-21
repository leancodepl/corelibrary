using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.Mixpanel;

[SuppressMessage(
    "StyleCop.CSharp.LayoutRules",
    "SA1507:CodeMustNotContainMultipleBlankLinesInARow",
    Justification = "Reviewed.")]
public class MixpanelAnalytics
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<MixpanelAnalytics>();

    private readonly HttpClient client;
    private readonly MixpanelConfiguration configuration;

    public MixpanelAnalytics(HttpClient client, MixpanelConfiguration configuration)
    {
        this.configuration = configuration;
        this.client = client;
    }

    public Task AliasAsync(string newId, string oldId, CancellationToken cancellationToken = default)
    {
        return TrackAsync(newId, "$create_alias", new Dictionary<string, object>()
        {
            ["distinct_id"] = oldId,
            ["alias"] = newId,
        }, cancellationToken: cancellationToken);
    }

    public Task TrackAsync(
        string userId,
        IMixpanelEvent mixpanelEvent,
        bool isImport = false,
        CancellationToken cancellationToken = default)
        => TrackAsync(userId, mixpanelEvent.EventName, mixpanelEvent.Properties, isImport, cancellationToken);

    public Task TrackAsync(
        string userId,
        string eventName,
        string propertyName,
        string propertyValue,
        bool isImport = false,
        CancellationToken cancellationToken = default)
        => TrackAsync(userId, eventName, new Dictionary<string, object>() { [propertyName] = propertyValue }, isImport, cancellationToken);

    public Task TrackAsync(
        string userId,
        string eventName,
        Dictionary<string, object> properties,
        bool isImport = false,
        CancellationToken cancellationToken = default)
        => TrackEventAsync(userId, eventName, properties, isImport, cancellationToken);


    public Task SetAsync(string userId, string name, string value, CancellationToken cancellationToken = default)
        => SetAsync(userId, new Dictionary<string, string>() { [name] = value }, cancellationToken);

    public Task SetAsync(string userId, Dictionary<string, string> properties, CancellationToken cancellationToken = default)
        => EngageAsync(userId, "$set", properties, cancellationToken);


    public Task AddAsync(string userId, string name, string value, CancellationToken cancellationToken = default)
        => AddAsync(userId, new Dictionary<string, string>() { [name] = value }, cancellationToken);

    public Task AddAsync(string userId, Dictionary<string, string> properties, CancellationToken cancellationToken = default)
        => EngageAsync(userId, "$add", properties, cancellationToken);


    public Task AppendAsync(string userId, string name, object value, CancellationToken cancellationToken = default)
        => AppendAsync(userId, new Dictionary<string, object>() { [name] = value }, cancellationToken);

    public Task AppendAsync(string userId, Dictionary<string, object> properties, CancellationToken cancellationToken = default)
        => EngageAsync(userId, "$append", properties, cancellationToken);


    public Task SetOnceAsync(string userId, string name, string value, CancellationToken cancellationToken = default)
        => SetOnceAsync(userId, new Dictionary<string, string>() { [name] = value }, cancellationToken);

    public Task SetOnceAsync(string userId, Dictionary<string, string> properties, CancellationToken cancellationToken = default)
        => EngageAsync(userId, "$set_once", properties, cancellationToken);


    public Task UnionAsync(string userId, string name, List<string> elements, CancellationToken cancellationToken = default)
        => UnionAsync(userId, new Dictionary<string, List<string>>() { [name] = elements }, cancellationToken);

    public Task UnionAsync(
        string userId,
        Dictionary<string, List<string>> properties, CancellationToken cancellationToken = default)
        => EngageAsync(userId, "$union", properties, cancellationToken);


    public Task UnsetAsync(string userId, string property, CancellationToken cancellationToken = default)
        => UnsetAsync(userId, new List<string>() { property }, cancellationToken);

    public Task UnsetAsync(string userId, List<string> properties, CancellationToken cancellationToken = default)
        => EngageAsync(userId, "$unset", properties, cancellationToken);


    public Task DeleteAsync(string userId, CancellationToken cancellationToken = default)
        => EngageAsync(userId, "$delete", cancellationToken: cancellationToken);


    private Task TrackEventAsync(
        string userId,
        string name,
        Dictionary<string, object>? properties,
        bool isImport,
        CancellationToken cancellationToken)
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

        return MakeRequestAsync(userId, isImport ? "import" : "track", name, data, cancellationToken);
    }

    private Task EngageAsync(
        string userId,
        string operation,
        object? properties = null,
        CancellationToken cancellationToken = default)
    {
        logger.Verbose("Engaging Mixpanel operation {Name} for user {UserId}", operation, userId);
        var data = new Dictionary<string, object?>()
        {
            ["$token"] = configuration.Token,
            ["$distinct_id"] = userId,
            [operation] = properties,
        };

        return MakeRequestAsync(userId, "engage", operation, data, cancellationToken);
    }

    private async Task MakeRequestAsync(
        string userId,
        string uri,
        string requestName,
        Dictionary<string, object?> data,
        CancellationToken cancellationToken)
    {
        var dataString = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(data));
        var url = $"{uri}/?data={dataString}&verbose={(configuration.VerboseErrors ? "1" : "0")}&api_key={configuration.ApiKey}";

        using var rawResponse = await client.GetAsync(url, cancellationToken);
        var content = await rawResponse.Content.ReadAsStringAsync(cancellationToken);
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

            if (response?.Status == MixpanelResponse.Success)
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
