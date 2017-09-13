using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using LeanCode.UnitTests.TestHelpers;
using Xunit;

namespace LeanCode.DomainModels.EventsExecution.Tests.TestHelpers
{
    public abstract class EventsInterceptorTests_RaceCondsBase
    {
        const int Iterations = 1000;

        [Fact(Skip="a")]
        public Task Clash_Async_Test1() => RunTaskTest();
        [Fact(Skip="a")]
        public Task Clash_Async_Test2() => RunTaskTest();
        [Fact(Skip="a")]
        public Task Clash_Async_Test3() => RunTaskTest();
        [Fact(Skip="a")]
        public void Clash_Thread_Test1() => RunThreadTest();
        [Fact(Skip="a")]
        public void Clash_Thread_Test2() => RunThreadTest();
        [Fact(Skip="a")]
        public void Clash_Thread_Test3() => RunThreadTest();

        private static async Task RunTaskTest()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var inter = EventsInterceptor.Single<SampleEvent1>();
                var id = Guid.NewGuid();
                await Task.Yield();
                DomainEvents.Raise(new SampleEvent1(id));

                Assert.True(inter.Raised);
                Assert.Equal(id, inter.Event.Id);
            }
        }

        private static void RunThreadTest()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var inter = EventsInterceptor.Single<SampleEvent1>();
                var id = Guid.NewGuid();
                DomainEvents.Raise(new SampleEvent1(id));

                Assert.True(inter.Raised);
                Assert.Equal(id, inter.Event.Id);
            }
        }
    }

    public class EventsInterceptorTests_RaceConds1 : EventsInterceptorTests_RaceCondsBase
    { }

    public class EventsInterceptorTests_RaceConds2 : EventsInterceptorTests_RaceCondsBase
    { }
}
