using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using LeanCode.Test.Helpers;
using LeanCode.UnitTests.TestHelpers;
using Xunit;

namespace LeanCode.DomainModels.EventsExecution.TestHelpers.Tests;

[Collection("EventsInterceptor")]
public abstract class EventsInterceptorTests_RaceCondsBase
{
    private const int Iterations = 1000;

    protected EventsInterceptorTests_RaceCondsBase()
    {
        EventsInterceptor.Configure();
    }

    [LongRunningFact]
    public Task Clash_Async_Test1() => RunTaskTest();

    [LongRunningFact]
    public Task Clash_Async_Test2() => RunTaskTest();

    [LongRunningFact]
    public Task Clash_Async_Test3() => RunTaskTest();

    [LongRunningFact]
    public void Clash_Thread_Test1() => RunThreadTest();

    [LongRunningFact]
    public void Clash_Thread_Test2() => RunThreadTest();

    [LongRunningFact]
    public void Clash_Thread_Test3() => RunThreadTest();

    private static async Task RunTaskTest()
    {
        for (var i = 0; i < Iterations; i++)
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
        for (var i = 0; i < Iterations; i++)
        {
            var inter = EventsInterceptor.Single<SampleEvent1>();
            var id = Guid.NewGuid();
            DomainEvents.Raise(new SampleEvent1(id));

            Assert.True(inter.Raised);
            Assert.Equal(id, inter.Event.Id);
        }
    }
}

public class EventsInterceptorTests_RaceConds1 : EventsInterceptorTests_RaceCondsBase { }

public class EventsInterceptorTests_RaceConds2 : EventsInterceptorTests_RaceCondsBase { }
