using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using LeanCode.Kratos.Model;
using Microsoft.AspNetCore.Http;

namespace LeanCode.Kratos;

public sealed record class KratosWebHookHandlerConfig(string ApiKey);

public abstract partial class KratosWebHookHandlerBase
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<KratosWebHookHandlerBase>();

    private readonly KratosWebHookHandlerConfig config;

    protected KratosWebHookHandlerBase(KratosWebHookHandlerConfig config)
    {
        this.config = config;
    }

    protected virtual string ApiKeyHeaderName => "X-Api-Key";

    protected abstract Task HandleCoreAsync(HttpContext ctx);

    [SuppressMessage(
        "?",
        "CA1031",
        Justification = "Exception is logged, rethrowing it for ASP.NET to handle is unnecessary."
    )]
    public async Task HandleAsync(HttpContext ctx)
    {
        try
        {
            if (VerifyApiKey(ctx.Request.Headers[ApiKeyHeaderName].ToString(), config.ApiKey))
            {
                await HandleCoreAsync(ctx);
            }
            else
            {
                logger.Error("Invalid Api Key");
                ctx.Response.StatusCode = 403;
            }
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to process webhook");
            ctx.Response.StatusCode = 500;
        }
    }

    private static bool VerifyApiKey(string left, string right)
    {
        var leftBytes = MemoryMarshal.AsBytes(left.AsSpan());
        var rightBytes = MemoryMarshal.AsBytes(right.AsSpan());

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    public static Task WriteIdentityResponseAsync(HttpContext ctx, Identity identity)
    {
        ctx.Response.StatusCode = 200;
        return ctx.Response.WriteAsJsonAsync(
            new(identity),
            KratosWebHookHandlerBaseJsonSerializerContext.Default.IdentityResponseBody,
            cancellationToken: ctx.RequestAborted
        );
    }

    public static Task WriteErrorResponseAsync(HttpContext ctx, List<ErrorMessage> messages, int statusCode = 400)
    {
        ctx.Response.StatusCode = statusCode;
        return ctx.Response.WriteAsJsonAsync(
            new(messages),
            KratosWebHookHandlerBaseJsonSerializerContext.Default.ErrorResponseBody,
            cancellationToken: ctx.RequestAborted
        );
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(IdentityResponseBody))]
[JsonSerializable(typeof(ErrorResponseBody))]
public partial class KratosWebHookHandlerBaseJsonSerializerContext : JsonSerializerContext;

public record struct IdentityResponseBody([property: JsonPropertyName("identity")] Identity Identity);

public record struct ErrorResponseBody(
    [SuppressMessage("?", "CA2227")] [property: JsonPropertyName("messages")] List<ErrorMessage> Messages
);

public record struct ErrorMessage(
    [property: JsonPropertyName("instance_ptr")] string? InstancePtr,
    [SuppressMessage("?", "CA2227")] [property: JsonPropertyName("messages")] List<DetailedMessage> Messages
);

public record struct DetailedMessage(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("context")] JsonElement? Context
);
