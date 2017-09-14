using System;
using LeanCode.DomainModels.Model;
using LeanCode.UnitTests.TestHelpers;
using Xunit;

namespace LeanCode.DomainModels.EventsExecution.TestHelpers.Tests
{
    public class EventsInterceptorTests
    {
        public EventsInterceptorTests()
        {
            EventsInterceptor.Configure();
        }

        [Fact]
        public void Multiple_events_are_intercepted_correctly()
        {
            var e1 = EventsInterceptor.Single<SampleEvent1>();
            var e2 = EventsInterceptor.Single<SampleEvent2>();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            DomainEvents.Raise(new SampleEvent1(id1));
            DomainEvents.Raise(new SampleEvent2(id2));

            Assert.True(e1.Raised);
            Assert.True(e2.Raised);
            Assert.Equal(id1, e1.Event.Id);
            Assert.Equal(id2, e2.Event.Id);
        }

        [Fact]
        public void If_multiple_events_are_raised_Last_is_intercepted()
        {
            var e = EventsInterceptor.Single<SampleEvent1>();
            var id = Guid.NewGuid();

            DomainEvents.Raise(new SampleEvent1(Guid.NewGuid()));
            DomainEvents.Raise(new SampleEvent1(id));

            Assert.True(e.Raised);
            Assert.Equal(id, e.Event.Id);
        }

        [Fact]
        public void Single_ignores_other_events()
        {
            var e = EventsInterceptor.Single<SampleEvent1>();
            var id = Guid.NewGuid();

            DomainEvents.Raise(new SampleEvent1(id));
            DomainEvents.Raise(new SampleEvent2(Guid.NewGuid()));

            Assert.True(e.Raised);
            Assert.Equal(id, e.Event.Id);
        }
    }
}
