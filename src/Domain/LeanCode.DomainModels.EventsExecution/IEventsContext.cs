using System;
using System.Collections.Generic;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution
{
    public interface IEventsInterceptorContext : IPipelineContext
    {
        List<IDomainEvent> SavedEvents { get; set; }
    }

    public interface IEventsExecutorContext : IPipelineContext
    {
        List<(IDomainEvent Event, Type Handler)> ExecutedHandlers { get; set; }
        List<(IDomainEvent Event, Type Handler)> FailedHandlers { get; set; }
    }

    public interface IEventsContext : IEventsInterceptorContext, IEventsExecutorContext
    { }
}
