# Messaging - MassTransit

[MassTransit] is a popular open-source distributed application framework for building scalable and robust messaging systems in .NET applications. It provides a comprehensive set of tools and abstractions to simplify the development of message-based applications, making it easier to implement messaging patterns.

To integrate [MassTransit] with LeanCode CoreLibrary CQRS, you can utilize the [LeanCode.CQRS.MassTransitRelay] package. This package serves as a bridge that enables the passing of raised events from your application to the [MassTransit] message bus. This integration is vital for facilitating event-driven communication within domain.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.CQRS.MassTransitRelay | [![NuGet version (LeanCode.CQRS.MassTransitRelay)](https://img.shields.io/nuget/vpre/LeanCode.ConfigCat.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.MassTransitRelay) | Configuration |
| MassTransit | [![NuGet version (MassTransit)](https://img.shields.io/nuget/v/MassTransit.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/MassTransit) | Configuration |

## Configuration

[LeanCode.CQRS.MassTransitRelay] requires the following elements to be configured in the CQRS pipeline (in the following order):

1. `CommitDatabaseTransactionMiddleware` (call [CommitTransaction])
2. `EventsPublisherMiddleware` (call [PublishEvents])

!!! tip
    To find you more how you can configure pipeline visit [here](../../cqrs/pipeline/index.md).

For setting up bus configuration, [AddMassTransitIntegration] must be used. This method registers all the essential services required for [MassTransit] to work seamlessly with the rest of CoreLibrary. It effectively invokes [AddMassTransit], allowing you to consult the [MassTransit documentation](https://masstransit.io/documentation/concepts) for further insights.

Here's an example configuration that utilizes in-memory transport during development and Azure Service Bus in non-development environments:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // . . .

    services.AddCQRSMassTransitIntegration(cfg =>
    {
        // Adds MassTransit's Inbox & Outbox pattern implementation.
        //  More information in Inbox & Outbox section below.
        cfg.AddEntityFrameworkOutbox<CoreDbContext>(outboxCfg =>
        {
            outboxCfg.LockStatementProvider =
                // Using CustomPostgresLockStatementProvider vendored from
                // https://github.com/MassTransit/MassTransit/blob/9e6c78573ad211a70b624fad31382faa331dc4d8/src/Persistence/MassTransit.EntityFrameworkIntegration/EntityFrameworkIntegration/SqlLockStatementProvider.cs
                // as MassTransit uses EF8 incompatible API
                new LeanCode.CQRS.MassTransitRelay.LockProviders.CustomPostgresLockStatementProvider();
            outboxCfg.UseBusOutbox();
        });

        // Adds consumers with default configuration.
        // More information in Consumer definition section below.
        cfg.AddConsumersWithDefaultConfiguration(
            // Array of assemblies where handlers are located
            AllHandlers.Assemblies.ToArray(),
            typeof(DefaultConsumerDefinition<>)
        );

        if (hostEnv.IsDevelopment())
        {
            cfg.AddDelayedMessageScheduler();
            cfg.UsingInMemory(
                (ctx, cfg) =>
                {
                    cfg.UseDelayedMessageScheduler();
                    ConfigureBusCommon<IInMemoryBusFactoryConfigurator, IInMemoryReceiveEndpointConfigurator>(
                        ctx,
                        cfg
                    );
                }
            );
        }
        else
        {
            cfg.AddServiceBusMessageScheduler();
            cfg.UsingAzureServiceBus(
                (ctx, cfg) =>
                {
                    cfg.Host(
                        // Azure Service Bus endpoint taken from Configuration
                        new Uri(Config.MassTransit.AzureServiceBus.Endpoint(
                            Configuration)),
                        host =>
                        {
                            host.RetryLimit = 5;
                            host.RetryMinBackoff = TimeSpan.FromSeconds(3);
                            // Helper method to create Azure.Core.TokenCredential from Configuration
                            host.TokenCredential =
                                DefaultLeanCodeCredential.Create(Configuration);
                        }
                    );

                    cfg.UseServiceBusMessageScheduler();
                    ConfigureBusCommon<IServiceBusBusFactoryConfigurator, IServiceBusReceiveEndpointConfigurator>(
                        ctx,
                        cfg
                    );
                }
            );
        }

        static void ConfigureBusCommon<TConfigurator, TReceiveConfigurator>(
            IBusRegistrationContext ctx,
            TConfigurator cfg
        )
            where TConfigurator : IBusFactoryConfigurator<TReceiveConfigurator>
            where TReceiveConfigurator : IReceiveEndpointConfigurator
        {
            cfg.ConfigureEndpoints(ctx);
            cfg.ConfigureJsonSerializerOptions(KnownConverters.AddAll);
            cfg.ConnectBusObservers(ctx);
        }
    });

    // . . .
}
```

### Inbox & Outbox

The Inbox & Outbox pattern is a crucial architectural concept when it comes to handling and managing distributed transactions and messaging in software systems. In the context of CoreLibrary and [MassTransit] integration, this pattern is employed to ensure reliable and transactional message processing.

**Inbox:** The Inbox component serves as the place where incoming messages are initially received and stored before they are processed. It acts as a buffer, ensuring that no messages are lost or duplicated during the message processing pipeline. This is essential for achieving message reliability and consistency, especially in scenarios involving distributed systems.

**Outbox:** On the other hand, the Outbox is a key component for ensuring the atomicity of operations that involve both message publishing and database changes. It is used to store messages that need to be sent as part of a transaction. These messages are held in the Outbox until the associated database changes are committed successfully. Once the database transaction is confirmed, the Outbox releases the messages for delivery, ensuring that the two operations—database updates and message publishing—occur atomically. This is a critical feature when building systems that demand consistency across multiple components and data stores.

The integration of CoreLibrary and [MassTransit] relies on the implementation of the [MassTransit] Inbox & Outbox pattern. To learn more about this pattern, you can refer to [the documentation](https://masstransit.io/documentation/patterns/transactional-outbox). In addition to adding this pattern within `AddCQRSMassTransitIntegration` in your configuration, it also needs to be incorporated within `OnModelCreating` when using Entity Framework, as demonstrated below:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // . . .

    builder.AddTransactionalOutboxEntities();

    // . . .
}
```

### Consumer definition

[MassTransit] uses [ConsumerDefinition] to configure the pipeline of each consumer. To work effectively with domain events, the pipeline for every consumer that might raise them needs to include [EventsPublisherFilter]. For streamlined configuration and error prevention, [AddConsumersWithDefaultConfiguration] registers all consumers with default configuration using conventions. It's important to note that consumers registered this way **must be** public.

[LeanCode.CQRS.MassTransitRelay] comes with a set of filters, primarily designed to ensure that events raised in consumers are relayed to the bus. These filters include:

- [CorrelationFilter]: Enriches logs with a message ID and a consumer type, added by `UseLogsCorrelation`.
- [EventsPublisherFilter]: Relays Domain Events raised later in the pipeline to the bus, added by `UseDomainEventsPublishing`.

Here's a minimal, usable consumer definition that can serve as a default:

```csharp
public class DefaultConsumerDefinition<TConsumer>
    : ConsumerDefinition<TConsumer>
    where TConsumer : class, IConsumer
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TConsumer> consumerConfigurator,
        IRegistrationContext context
    )
    {
        // Correlate logs with the message consumer execution
        endpointConfigurator.UseLogsCorrelation();

        // Use transactional outbox pattern
        endpointConfigurator.UseEntityFrameworkOutbox<DbContext>(context);

        // Configure domain events
        endpointConfigurator.UseDomainEventsPublishing(context);
    }
}
```

[MassTransit]: https://masstransit-project.com/
[LeanCode.CQRS.MassTransitRelay]: https://github.com/leancodepl/corelibrary/tree/HEAD/src/CQRS/LeanCode.CQRS.MassTransitRelay
[CommitTransaction]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRelayApplicationBuilderExtensions.cs#L9
[Publishevents]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRelayApplicationBuilderExtensions.cs#L16
[AddMassTransitIntegration]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRelayServiceCollectionExtensions.cs#L10
[AddConsumersWithDefaultConfiguration]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRegistrationConfigurationExtensions.cs#L13
[CorrelationFilter]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.MassTransitRelay/Middleware/CorrelationFilter.cs
[EventsPublisherFilter]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.MassTransitRelay/Middleware/EventsPublisherFilter.cs
[ConsumerDefinition]: https://masstransit.io/documentation/configuration/consumers#consumer-definitions
[AddMassTransit]: https://masstransit.io/documentation/configuration
