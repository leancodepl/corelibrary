using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.MassTransitRelay;

public static class AsyncEventsInterceptorExtensions
{
    public static async Task<(TResult Result, List<IDomainEvent> Events)> CaptureEventsOfAsync<TResult>(
        this AsyncEventsInterceptor interceptor,
        Func<Task<TResult>> action
    )
    {
        interceptor.Prepare();
        var result = await action();
        var interceptedEvents = interceptor.CaptureQueue()?.ToList() ?? new List<IDomainEvent>();

        return (result, interceptedEvents);
    }

    public static async Task<List<IDomainEvent>> CaptureEventsOfAsync(
        this AsyncEventsInterceptor interceptor,
        Func<Task> action
    )
    {
        interceptor.Prepare();
        await action();
        var interceptedEvents = interceptor.CaptureQueue()?.ToList() ?? new List<IDomainEvent>();

        return interceptedEvents;
    }
}
