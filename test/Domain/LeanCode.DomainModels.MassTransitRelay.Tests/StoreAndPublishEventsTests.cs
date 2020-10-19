using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.DomainModels.Model;
using LeanCode.IdentityProvider;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    public class StoreAndPublishEventsTests : IDisposable
    {
        private static readonly Guid Event1Id = Identity.NewId();
        private static readonly Guid Event2Id = Identity.NewId();
        private static readonly Guid CorrelationId = Identity.NewId();

        private readonly EventsStore impl;
        private readonly TestDbContext dbContext;
        private readonly IRaisedEventsSerializer eventsSerializer;
        private readonly IEventPublisher publisher;
        private readonly List<IDomainEvent> domainEvents;

        public StoreAndPublishEventsTests()
        {
            dbContext = new TestDbContext();
            dbContext.Database.GetDbConnection().Open();
            dbContext.Database.EnsureCreated();

            publisher = Substitute.For<IEventPublisher>();
            eventsSerializer = new MockSerializer();
            impl = new EventsStore(dbContext, eventsSerializer);

            domainEvents = new List<IDomainEvent>
            {
                new Event1 { Id = Event1Id },
                new Event2 { Id = Event2Id },
            };
        }

        [Fact]
        public async Task Persists_events_and_marks_published_if_publish_succeeded()
        {
            await impl.StoreAndPublishEventsAsync(domainEvents, CorrelationId, publisher);

            var raisedEvents = await GetRaisedEvents();
            Assert.Collection(
                raisedEvents,
                evt => AssertRaisedEvent(evt, Event1Id, typeof(Event1), true),
                evt => AssertRaisedEvent(evt, Event2Id, typeof(Event2), true));
        }

        [Fact]
        public async Task Persists_event_but_does_not_marks_published_if_publish_failed()
        {
            publisher.PublishAsync(null, null).ReturnsForAnyArgs(_ => throw new Exception());

            await impl.StoreAndPublishEventsAsync(domainEvents, CorrelationId, publisher);

            var raisedEvents = await GetRaisedEvents();
            Assert.Collection(
                raisedEvents,
                evt => AssertRaisedEvent(evt, Event1Id, typeof(Event1), false),
                evt => AssertRaisedEvent(evt, Event2Id, typeof(Event2), false));
        }

        [Fact]
        public async Task One_interupted_publish_does_not_affect_consecutive_ones()
        {
            publisher.PublishAsync(null, null).ReturnsForAnyArgs(
                _ => throw new Exception(),
                _ => Task.CompletedTask);

            await impl.StoreAndPublishEventsAsync(domainEvents, CorrelationId, publisher);

            var raisedEvents = await GetRaisedEvents();
            Assert.Collection(
                raisedEvents,
                evt => AssertRaisedEvent(evt, Event1Id, typeof(Event1), false),
                evt => AssertRaisedEvent(evt, Event2Id, typeof(Event2), true));
        }

        private Task<List<RaisedEvent>> GetRaisedEvents()
        {
            return dbContext.RaisedEvents
                .OrderBy(evt => evt.EventType)
                .ToListAsync();
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        private static void AssertRaisedEvent(RaisedEvent evt, Guid id, Type type, bool wasPublished)
        {
            Assert.Equal(id, evt.Id);
            Assert.Equal(type.FullName, evt.EventType);
            Assert.Equal(wasPublished, evt.WasPublished);
        }

        private class Event1 : IDomainEvent
        {
            public Guid Id { get; set; }
            public DateTime DateOccurred { get; set; }
        }

        private class Event2 : IDomainEvent
        {
            public Guid Id { get; set; }
            public DateTime DateOccurred { get; set; }
        }

        private class MockSerializer : IRaisedEventsSerializer
        {
            public object ExtractEvent(RaisedEvent evt) => throw new NotImplementedException();

            public RaisedEvent WrapEvent(object evt, Guid correlationId)
            {
                return RaisedEvent.Create(evt, correlationId, _ => "mock_payload");
            }
        }
    }
}
