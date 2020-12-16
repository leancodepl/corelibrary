using System;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.DomainModels.Model;
using LeanCode.IdentityProvider;
using LeanCode.Time;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    public class NewtonsoftJsonEventsSerializerTests
    {
        private static readonly Guid EventId = Identity.NewId();
        private static readonly Guid CorrelationId = Identity.NewId();
        private static readonly DateTime DateOccurred = new(2020, 5, 7, 11, 0, 0, 0, DateTimeKind.Utc);

        private readonly NewtonsoftJsonEventsSerializer serializer;

        public NewtonsoftJsonEventsSerializerTests()
        {
            serializer = new NewtonsoftJsonEventsSerializer(new TypesCatalog(typeof(NewtonsoftJsonEventsSerializerTests)));
        }

        [Fact]
        public void Serializes_an_event_keeps_metadata()
        {
            var evt = new TestEvent(EventId, DateOccurred, 5);

            var raised = serializer.WrapEvent(evt, CorrelationId);

            Assert.Equal(EventId, raised.Id);
            Assert.Equal(CorrelationId, raised.CorrelationId);
            Assert.Equal(DateOccurred, raised.DateOcurred);
            Assert.Equal(typeof(TestEvent).FullName, raised.EventType);
            Assert.False(raised.WasPublished);
            Assert.Contains(@"""Value"":5", raised.Payload);
        }

        [Fact]
        public void Deserializes_event()
        {
            var raisedEvt = new RaisedEvent(
                EventId,
                CorrelationId,
                DateOccurred,
                false,
                typeof(TestEvent).FullName,
                @"{ ""Value"":5 }");

            var evt = serializer.ExtractEvent(raisedEvt);

            var typed = evt as TestEvent;
            Assert.NotNull(typed);
            Assert.Equal(5, typed.Value);
        }

        [Fact]
        public void Serializes_and_deserializes_events_correctly()
        {
            var evt = new TestEvent(EventId, DateOccurred, 5);

            var wrapped = serializer.WrapEvent(evt, CorrelationId);
            var unwrapped = serializer.ExtractEvent(wrapped);

            Assert.Equal(evt, unwrapped);
        }

        [Fact]
        public void Serializes_and_deserializes_events_with_private_setters_and_no_ctor_correctly()
        {
            var evt = TestEventWithPrivateFields.Create("test value");

            var wrapped = serializer.WrapEvent(evt, CorrelationId);
            var unwrapped = serializer.ExtractEvent(wrapped);

            Assert.Equal(evt, unwrapped);
        }

        public record TestEvent : IDomainEvent
        {
            public TestEvent(Guid id, DateTime dateOccurred, int value)
            {
                Id = id;
                DateOccurred = dateOccurred;
                Value = value;
            }

            public Guid Id { get; private set; }
            public DateTime DateOccurred { get; private set; }
            public int Value { get; private set; }
        }

        public record TestEventWithPrivateFields : IDomainEvent
        {
            public Guid Id { get; private init; }
            public DateTime DateOccurred { get; private init; }
            public string Value { get; private init; }

            public static TestEventWithPrivateFields Create(string val) => new(val);

            private TestEventWithPrivateFields(string val)
            {
                Id = Identity.NewId();
                DateOccurred = TimeProvider.Now;
                Value = val;
            }

            private TestEventWithPrivateFields()
            {
                Value = string.Empty;
            }
        }
    }
}
