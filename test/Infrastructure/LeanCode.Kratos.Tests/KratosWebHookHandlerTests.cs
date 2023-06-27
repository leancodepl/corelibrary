using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using LeanCode.Kratos.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace LeanCode.Kratos.Tests;

public class KratosWebHookHandlerTests
{
    private sealed class KratosWebHookTestHandler : KratosWebHookHandlerBase
    {
        private readonly Func<HttpContext, Task> handler;

        public new string ApiKeyHeaderName => base.ApiKeyHeaderName;

        public KratosWebHookTestHandler(KratosWebHookHandlerConfig config, Func<HttpContext, Task> handler)
            : base(config)
        {
            this.handler = handler;
        }

        protected override Task HandleCoreAsync(HttpContext ctx) => handler(ctx);
    }

    private static readonly string ApiKey = Guid.NewGuid().ToString();

    private static KratosWebHookTestHandler PrepareHandler(Func<HttpContext, Task> handler)
    {
        return new(new(ApiKey), handler);
    }

    private static KratosWebHookTestHandler PrepareHandler(Action<HttpContext> handler)
    {
        return PrepareHandler(ctx =>
        {
            handler(ctx);
            return Task.CompletedTask;
        });
    }

    private static async Task<HttpContext> RunAsync(
        KratosWebHookTestHandler handler,
        Dictionary<string, StringValues> headers
    )
    {
        var features = new FeatureCollection();
        features.Set<IHttpRequestFeature>(new HttpRequestFeature() { Headers = new HeaderDictionary(headers) });
        features.Set<IHttpResponseFeature>(new HttpResponseFeature());
        features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(new MemoryStream()));
        var httpContext = new DefaultHttpContext(features);
        await handler.HandleAsync(httpContext);
        return httpContext;
    }

    [Fact]
    public async Task Responds_with_status_code_403_without_running_inner_handler_if_api_key_is_invalid()
    {
        var handler = PrepareHandler(ctx => throw new UnreachableException());
        var ctx = await RunAsync(handler, new() { [handler.ApiKeyHeaderName] = Guid.NewGuid().ToString() });

        Assert.Equal(403, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task Runs_inner_handler_if_api_key_is_valid()
    {
        var handler = PrepareHandler(ctx => ctx.Response.StatusCode = 200);
        var ctx = await RunAsync(handler, new() { [handler.ApiKeyHeaderName] = ApiKey });

        Assert.Equal(200, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task Swallows_unhandled_exceptions_thrown_by_inner_handler_and_responds_with_status_code_500()
    {
        var handler = PrepareHandler(ctx => throw new InvalidOperationException());
        var ctx = await RunAsync(handler, new() { [handler.ApiKeyHeaderName] = ApiKey });

        Assert.Equal(500, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task Correctly_serializes_identity_responses()
    {
        var email = "test@leancode.pl";
        var timestampString = "2023-06-26T21:37:42Z";
        var timestamp = DateTime.Parse(timestampString, CultureInfo.InvariantCulture).ToUniversalTime();

        var identity = new Identity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            SchemaId = "user",
            SchemaUrl = new("https://auth.local.lncd.pl/schemas/dXNlcg"),
            State = IdentityState.Active,
            RecoveryAddresses = new(1)
            {
                new()
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    CreatedAt = timestamp,
                    UpdatedAt = timestamp,
                    Via = AddressType.Email,
                    Value = email,
                },
            },
            VerifiableAddresses = new(1)
            {
                new()
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    CreatedAt = timestamp,
                    UpdatedAt = timestamp,
                    Via = AddressType.Email,
                    Value = email,
                    Status = AddressStatus.Completed,
                    VerifiedAt = timestamp,
                    Verified = true,
                },
            },
            Traits = JsonSerializer.SerializeToElement(new Dictionary<string, string> { ["email"] = email }),
            MetadataPublic = null,
            MetadataAdmin = null,
        };

        var handler = PrepareHandler(ctx => KratosWebHookHandlerBase.WriteIdentityResponseAsync(ctx, identity));
        var ctx = await RunAsync(handler, new() { [handler.ApiKeyHeaderName] = ApiKey });

        Assert.Equal(200, ctx.Response.StatusCode);

        ctx.Response.Body.Position = 0L;
        using var document = await JsonDocument.ParseAsync(ctx.Response.Body);

        var id = document.RootElement.GetProperty("identity"u8);
        Assert.Equal("00000000-0000-0000-0000-000000000001", id.GetProperty("id"u8).GetString());
        Assert.Equal(timestampString, id.GetProperty("created_at"u8).GetString());
        Assert.Equal(timestampString, id.GetProperty("updated_at"u8).GetString());
        Assert.Equal("user", id.GetProperty("schema_id"u8).GetString());
        Assert.Equal("https://auth.local.lncd.pl/schemas/dXNlcg", id.GetProperty("schema_url"u8).GetString());
        Assert.Equal("active", id.GetProperty("state"u8).GetString());
        Assert.Equal(email, id.GetProperty("traits"u8).GetProperty("email"u8).GetString());
        Assert.Equal(JsonValueKind.Null, id.GetProperty("metadata_public"u8).ValueKind);
        Assert.Equal(JsonValueKind.Null, id.GetProperty("metadata_admin"u8).ValueKind);

        var recoveryAddresses = id.GetProperty("recovery_addresses"u8);
        Assert.Equal(1, recoveryAddresses.GetArrayLength());

        var recoveryAddress = recoveryAddresses[0];
        Assert.Equal("00000000-0000-0000-0000-000000000002", recoveryAddress.GetProperty("id"u8).GetString());
        Assert.Equal(timestampString, recoveryAddress.GetProperty("created_at"u8).GetString());
        Assert.Equal(timestampString, recoveryAddress.GetProperty("updated_at"u8).GetString());
        Assert.Equal("email", recoveryAddress.GetProperty("via"u8).GetString());
        Assert.Equal(email, recoveryAddress.GetProperty("value"u8).GetString());

        var verifiableAddresses = id.GetProperty("verifiable_addresses"u8);
        Assert.Equal(1, verifiableAddresses.GetArrayLength());

        var verifiableAddress = verifiableAddresses[0];
        Assert.Equal("00000000-0000-0000-0000-000000000003", verifiableAddress.GetProperty("id"u8).GetString());
        Assert.Equal(timestampString, verifiableAddress.GetProperty("created_at"u8).GetString());
        Assert.Equal(timestampString, verifiableAddress.GetProperty("updated_at"u8).GetString());
        Assert.Equal("email", verifiableAddress.GetProperty("via"u8).GetString());
        Assert.Equal(email, verifiableAddress.GetProperty("value"u8).GetString());
        Assert.Equal("completed", verifiableAddress.GetProperty("status"u8).GetString());
        Assert.Equal(timestampString, verifiableAddress.GetProperty("verified_at"u8).GetString());
        Assert.True(verifiableAddress.GetProperty("verified"u8).GetBoolean());
    }
}
