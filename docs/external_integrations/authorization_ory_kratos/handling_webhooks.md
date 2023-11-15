# Handling webhooks

To effectively manage incoming webhooks from Ory Kratos, you can employ the [KratosWebHookHandlerBase] class. The following example demonstrates how to synchronize identity data received from Kratos through webhooks with your database using [MassTransit]. This assumes that your Kratos instance is properly configured to send webhooks to your application.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.Kratos | [![NuGet version (LeanCode.Kratos)](https://img.shields.io/nuget/vpre/LeanCode.Kratos.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.Kratos/8.0.2260-preview/) | Webhook handling |

## Mapping the Endpoint in Application Configuration

First, you should map the `KratosIdentitySyncHandler` class to the `/kratos/sync-identity` endpoint in your application's configuration:

```csharp
protected override void ConfigureApp(IApplicationBuilder app)
{
    // . . .

    app.UseAuthentication()
        .UseEndpoints(endpoints =>
        {
            // . . .

            endpoints.MapPost(
                "/kratos/sync-identity",
                ctx => ctx.RequestServices
                    .GetRequiredService<KratosIdentitySyncHandler>()
                    .HandleAsync(ctx)
            );

            // . . .
        });

    // . . .
}
```

## Defining the Webhook Handler

Then, you can define the webhook handler to handle incoming requests. In this example, we deserialize request into [Identity] class, verify it inside a webhook and send an event that the identity has been updated, which will be handled by the appropriate `IConsumer` using [MassTransit]:

```csharp
public partial class KratosIdentitySyncHandler : KratosWebHookHandlerBase
{
    private readonly IBus bus;

    public KratosIdentitySyncHandler(
        KratosWebHookHandlerConfig config,
        IBus bus)
        : base(config)
    {
        this.bus = bus;
    }

    protected override async Task HandleCoreAsync(HttpContext ctx)
    {
        // Deserialize request into `Identity` class
        // from `LeanCode.Kratos` package
        var body = await JsonSerializer.DeserializeAsync(
            ctx.Request.Body,
            KratosIdentitySyncHandlerContext.Default.RequestBody,
            ctx.RequestAborted
        );

        var identity = body.Identity;

        if (identity is null)
        {
            // Helper method defined in KratosWebHookHandlerBase
            await WriteErrorResponseAsync(
                ctx,
                new List<ErrorMessage>
                {
                    new ErrorMessage(
                        null,
                        new List<DetailedMessage>
                        {
                            new DetailedMessage(
                                1,
                                "identity is null",
                                "error",
                                null),
                        }),
                },
                422
            );
            return;
        }
        else if (identity.Id == default)
        {
            await WriteErrorResponseAsync(
                ctx,
                new List<ErrorMessage>
                {
                    new ErrorMessage(
                        null,
                        new List<DetailedMessage>
                        {
                            new DetailedMessage(
                                2,
                                "identity.id is empty",
                                "error",
                                null),
                        }),
                },
                422
            );
            return;
        }

        // Send a message via MassTransit that identity has been updated
        await bus.Publish(
            new KratosIdentityUpdated(Guid.NewGuid(), Time.UtcNow, identity),
            ctx.RequestAborted);

        ctx.Response.StatusCode = 200;
    }

    public record struct RequestBody(
        [property: JsonPropertyName("identity")] Identity? Identity);

    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
    [JsonSerializable(typeof(RequestBody))]
    private partial class KratosIdentitySyncHandlerContext
        : JsonSerializerContext { }
}
```

> **Tip:** To read about LeanCode CoreLibrary MassTranist integration visit [here](../messaging_masstransit/index.md).

## Updating the Database

With the `KratosIdentitySyncHandler` in place, you can process incoming Kratos webhooks and handle them appropriately. The following code snippet updates the database table that stores identities (assuming that `KratosIdentities` table is already present in your database) when a `KratosIdentityUpdated` event is received:

```csharp
public sealed record class KratosIdentityUpdated(
    Guid Id,
    DateTime DateOccurred,
    Identity Identity)
    : IDomainEvent;

public class SyncKratosIdentity : IConsumer<KratosIdentityUpdated>
{
    private readonly CoreDbContext dbContext;

    public SyncKratosIdentity(CoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<KratosIdentityUpdated> context)
    {
        var kratosIdentity = context.Message.Identity;
        var identityId = kratosIdentity.Id;

        var dbIdentity = await dbContext.KratosIdentities.FindAsync(
            keyValues: new[] { (object)identityId },
            context.CancellationToken
        );

        // Database transaction will be commited at the end of the pipeline
        // assuming `CommitDatabaseTransactionMiddleware` was added
        if (dbIdentity is null)
        {
            dbIdentity = new(kratosIdentity);
            dbContext.KratosIdentities.Add(dbIdentity);
        }
        else
        {
            dbIdentity.Update(kratosIdentity);
            dbContext.KratosIdentities.Update(dbIdentity);
        }
    }
}
```

> **Tip:** To read about handling events visit [here](../messaging_masstransit/handling_events.md) and to read about the pipeline visit [here](../../cqrs/pipeline/index.md)

[MassTransit]: https://masstransit-project.com/
[KratosWebHookHandlerBase]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/Infrastructure/LeanCode.Kratos/KratosWebHookHandlerBase.cs
[Identity]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/Infrastructure/LeanCode.Kratos/Model/Identity.cs
