# MassTransit Integration

[LeanCode.CQRS.MassTransitRelay] allows to pass raised events to [MassTransit](https://masstransit-project.com/) bus. This is the only way of consuming domain events.

## Configuration

Relay requires the following elements to be configured in the CQRS pipeline (in the following order):

1. `CommitDatabaseTransactionMiddleware` (call [CommitTransaction])
2. `EventsPublisherMiddleware` (call [PublishEvents])

[AddMassTransitIntegration] has to be used for bus configuration. It registers all the necessary services that allow it to work seamlessly with the rest of CoreLib. It calls [AddMassTransit](https://masstransit.io/documentation/configuration) underneath, so you can refer to [MassTransit documentation](https://masstransit.io/documentation/concepts) for more information.

You can use default consumer registration facilities that MassTransit provides to register consumers, or you can use [AddConsumersWithDefaultConfiguration] to register all consumers (with configuration) by convention. The consumers registered that way **have to be** `public`.

### Filters

Relay module comes with a set of filters, mainly to ensure that events raised in consumers are relayed to the bus again.
The filters are:

- [CorrelationFilter] - enriches logs with a message id and a consumer type,
- [EventsPublisherFilter] - relays Domain Events raised further in the pipeline to the bus.

### Minimal viable consumer definition

MassTransit uses [ConsumerDefinition](https://masstransit.io/documentation/configuration/consumers#consumer-definitions) to configure the pipeline of each consumer. To work properly with domain events, pipeline for every consumer that might raise them needs to include [EventsPublisherFilter]. To streamline configuration and prevent errors, [AddConsumersWithDefaultConfiguration] requires default configuration (that might be changed for each consumer). Here is the minimal usable consumer definition (that can be used as default one).

```csharp
public class DefaultConsumerDefinition<TConsumer> : ConsumerDefinition<TConsumer>
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
        // endpointConfigurator.UseEntityFrameworkOutbox<DbContext>(context);

        // Configure domain events
        endpointConfigurator.UseDomainEventsPublishing(context);
    }
}
```

## Inbox & Outbox

CoreLibrary + MassTransit integration relies on the Outbox pattern. Although CoreLibrary provided own inbox/outbox implementation, in v8 we switched to MassTransit one. Please refer to [the documentation](https://masstransit.io/documentation/patterns/transactional-outbox) to read how to integrate it.

[LeanCode.CQRS.MassTransitRelay]: https://github.com/leancodepl/corelibrary/tree/v8.0-preview/src/CQRS/LeanCode.CQRS.MassTransitRelay
[CommitTransaction]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRelayApplicationBuilderExtensions.cs#L9
[Publishevents]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRelayApplicationBuilderExtensions.cs#L16
[AddMassTransitIntegration]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRelayServiceCollectionExtensions.cs#L10
[AddConsumersWithDefaultConfiguration]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRegistrationConfigurationExtensions.cs#L13
[CorrelationFilter]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/CQRS/LeanCode.CQRS.MassTransitRelay/Middleware/CorrelationFilter.cs
[EventsPublisherFilter]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/CQRS/LeanCode.CQRS.MassTransitRelay/Middleware/EventsPublisherFilter.cs
