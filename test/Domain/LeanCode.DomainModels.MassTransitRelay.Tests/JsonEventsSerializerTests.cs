using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.DomainModels.Model;
using LeanCode.Time;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    public abstract class BaseJsonEventsSerializerTests
    {
        protected static readonly TypesCatalog Types = TypesCatalog.Of<BaseJsonEventsSerializerTests>();

        protected abstract IRaisedEventsSerializer Serializer { get; }

        private static readonly Guid EventId = Guid.NewGuid();
        private static readonly Guid ConversationId = Guid.NewGuid();
        private static readonly ActivityContext ActivityContext = new ActivityContext(
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.None);
        private static readonly DateTime DateOccurred = new DateTime(2020, 5, 7, 11, 0, 0, 0, DateTimeKind.Utc);
        private readonly RaisedEventMetadata metadata;

        public BaseJsonEventsSerializerTests()
        {
            metadata = new RaisedEventMetadata
            {
                ConversationId = ConversationId,
                ActivityContext = ActivityContext,
            };
        }

        [Fact]
        public void Serializes_an_event_keeps_metadata()
        {
            var evt = new TestEvent(EventId, DateOccurred, 5);

            var raised = Serializer.WrapEvent(evt, metadata);

            Assert.Equal(EventId, raised.Id);
            Assert.Equal(ConversationId, raised.Metadata.ConversationId);
            Assert.Equal(ActivityContext, raised.Metadata.ActivityContext);
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
                metadata,
                DateOccurred,
                false,
                typeof(TestEvent).FullName,
                @"{ ""Value"":5 }");

            var evt = Serializer.ExtractEvent(raisedEvt);

            var typed = evt as TestEvent;
            Assert.NotNull(typed);
            Assert.Equal(5, typed.Value);
        }

        [Fact]
        public void Serializes_and_deserializes_events_correctly()
        {
            var evt = new TestEvent(EventId, DateOccurred, 5);

            var wrapped = Serializer.WrapEvent(evt, metadata);
            var unwrapped = Serializer.ExtractEvent(wrapped);

            Assert.Equal(evt, unwrapped);
        }

        [Fact]
        public void Serializes_and_deserializes_events_with_private_setters_and_no_ctor_correctly()
        {
            var evt = TestEventWithPrivateFields.Create("test value");

            var wrapped = Serializer.WrapEvent(evt, metadata);
            var unwrapped = Serializer.ExtractEvent(wrapped);

            Assert.Equal(evt, unwrapped);
        }

        public record TestEvent(Guid Id, DateTime DateOccurred, int Value) : IDomainEvent;

        public record TestEventWithPrivateFields : IDomainEvent
        {
            public Guid Id { get; private init; }
            public DateTime DateOccurred { get; private init; }
            public string Value { get; private init; }

            public static TestEventWithPrivateFields Create(string val) => new(val);

            private TestEventWithPrivateFields(string val)
            {
                Id = Guid.NewGuid();
                DateOccurred = TimeProvider.Now;
                Value = val;
            }

            private TestEventWithPrivateFields()
            {
                Value = string.Empty;
            }
        }
    }

    public sealed class NewtonsoftJsonEventsSerializerTests : BaseJsonEventsSerializerTests
    {
        protected override IRaisedEventsSerializer Serializer { get; } = new NewtonsoftJsonEventsSerializer(Types);
    }

    // public sealed class SystemTextJsonEventsSerializerTests : BaseJsonEventsSerializerTests
    // {
    //     protected override IRaisedEventsSerializer Serializer { get; } = new SystemTextJsonEventsSerializer(Types);
    // }
}
