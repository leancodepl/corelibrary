# MassTransit Integration

`LeanCode.DomainModels.MassTransitRelay` allows to pass raised events to [MassTransit](https://masstransit-project.com/) bus. This is the only way of consuming domain events.

## Configuration

Relay requires the following elements to be configured in the CQRS pipeline (in the following order):

- `CorrelationElement`
- `StoreAndPublishEventsElement` (`EventsPublisherElement` if the outbox is not necessary).

Additionally, `MassTransitRelayModule` has to be registered, with the assembly catalogs for events and consumers and a bus factory method. The consumers registered that way **have to be** `public`. The bus factory method should specify transport and configure the bus in a regular MassTransit way (typically it would call `UsingInMemory/UsingAzureServiceBus` etc., see [Mass Transit docs](https://masstransit-project.com/usage/configuration.html#asp-net-core) for more information).

### Filters

Relay module comes with a set of filters, mainly to ensure that events raised in consumers are relayed to the bus again.
The filters are:

- `CorrelationFilter` - enriches logs with message ids, correlation ids and consumer types
- `ConsumeMessagesOnceFilter`\* - see the [inbox](#Inbox)
- `EventsPublisherFilter`\* - relays Domain Events raised further in the pipeline to the bus
- `StoreAndPublishEventsFilter`\* - as above, additionally implements [outbox](#Outbox)

CAUTION: Filters marked with \* are registered via configuration observers - they are applied after other regular filters.
This will cause an counterintuitive filters order if do not register them as last in the pipeline.

## Consumers

Events relayed to MassTransit are not consumed by `IDomainEventHandler<TEvent>` interfaces, instead the regular MassTransit
`IConsumer<TMessage>` consumers are used directly.

## Inbox

`ConsumeMessagesOnceFilter` ensures that messages are consumed at most once. This is important in case of replaying failed messages, given how Mass Transit works - all the messages are received through a single queue. So, in case of redelivery, all the consumers subscribed to a message will run, even those we don't want to.

Inbox to work requires an `IConsumedMessageContext` to be registered in the DI container.

Information about consuming is committed to the database along with business data. It is safe to commit database transactions directly in handlers when using inbox.

Inbox is periodically cleaned to remove outdated entries.

## Outbox

Outbox persists events raised in command handler or message consumer in the same transaction in the same transaction as business data. It allows to re publish the events in case bus goes down for some time.

The outbox works in the following fashion:

0. A handler performs business operation and raises events
1. Raised events are serialized and saved in the database
2. Database transaction is committed to the database (along with business data)
3. Events are sent to the bus
4. Events sent successfully are marked as published

Apart from this there is a background service (`PeriodicEventsPublisher`) periodically fetching and publishing yet not published events. Additionally, outdated published events are removed from the outbox -

For outbox to work `IOutboxContext` has to be registered in the DI container.

Important considerations when using outbox:

- Command handlers and consumers **cannot** commit their database transactions, otherwise events won't be persisted in the same transaction. Database commits will be handled by the pipeline.
- Mass Transit in memory outbox **cannot** be used along with our outbox, otherwise it will fool ours that events are published successfully, while being just acknowledged by the outbox
- The outbox works only for Domain Events. Any events published in consumers directly through `ConsumeContext` will be published immediately
- Enable duplicate detection on the bus - there might be cases when an event is published twice - via the CQRS pipeline and via `PeriodicEventsPublisher`
- For the same reason make sure to either combine Outbox with Inbox or make handlers idempotent - messages might come multiple times
