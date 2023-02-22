using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.SmsSender.Exceptions;

namespace LeanCode.SmsSender;

public class SmsApiClient : ISmsSender
{
    public const string ApiBase = "https://api.smsapi.pl";

    private static readonly ImmutableHashSet<int> ClientErrors = ImmutableHashSet.Create(
        101, /* Invalid or no authorization data */
        102, /* Invalid login or password */
        103, /* Shortage of points for this user */
        105, /* Invalid IP address */
        110, /* Service is not available on this account */
        1000, /* Action is available only for main user */
        1001 /* Invalid action */
    );

    private static readonly ImmutableHashSet<int> HostErrors = ImmutableHashSet.Create(
        8, /* Error in request */
        201, /* Internal system error */
        666, /* Internal system error */
        999 /* Internal system error */
    );

    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SmsApiClient>();

    private readonly SmsApiConfiguration config;
    private readonly HttpClient client;

    public static void ConfigureHttpClient(SmsApiConfiguration config, HttpClient client)
    {
        client.BaseAddress = new Uri(ApiBase);

        if (config.Token is { Length: > 0 } token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.Login}:{config.Password}"))
            );
        }
    }

    public SmsApiClient(SmsApiConfiguration config, HttpClient client)
    {
        this.config = config;
        this.client = client;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA2234",
        Justification = "Could potentialy change behavior."
    )]
    public async Task SendAsync(string message, string phoneNumber, CancellationToken cancellationToken = default)
    {
        logger.Verbose("Sending SMS using SMS Api");

        var parameters = new Dictionary<string, string?>
        {
            ["format"] = "json",
            ["encoding"] = "UTF-8", // documentation claims it's the default anyway, but let's keep it for safety
            ["from"] = config.From,
            ["to"] = phoneNumber,
            ["message"] = message,
        };

        if (config.TestMode)
        {
            parameters["test"] = "1";
        }

        if (config.FastMode)
        {
            parameters["fast"] = "1";
        }

        using var requestContent = new FormUrlEncodedContent(parameters!);
        using var response = await client.PostAsync("sms.do", requestContent, cancellationToken);

        await using var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            HandleResponse(doc.RootElement);
        }
        catch (Exception e) when (e is JsonException || e is KeyNotFoundException || e is FormatException)
        {
            throw new SerializationException("Failed to parse error message.", e);
        }

        logger.Information("SMS sent successfully");
    }

    private static void HandleResponse(JsonElement response)
    {
        if (response.TryGetProperty("error", out var error))
        {
            var errorCode = error.GetInt32();
            var errorMessage = response.GetProperty("message").GetString()!;

            if (IsClientError(errorCode))
            {
                throw new ClientException(errorCode, errorMessage);
            }
            else if (IsHostError(errorCode))
            {
                throw new HostException(errorCode, errorMessage);
            }
            else
            {
                throw new ActionException(errorCode, errorMessage);
            }
        }
    }

    private static bool IsClientError(int code) => ClientErrors.Contains(code);

    private static bool IsHostError(int code) => HostErrors.Contains(code);
}
