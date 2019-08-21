using System;
using System.Collections.Generic;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution
{
    /// <summary>Carries information about <see cref="IDomainEvent"/> raised and handled during pipeline execution</summary>
    public interface IEventsContext : IPipelineContext
    {
        List<IDomainEvent> SavedEvents { get; set; }
        List<(IDomainEvent Event, Type Handler)> ExecutedHandlers { get; set; }
        List<(IDomainEvent Event, Type Handler)> FailedHandlers { get; set; }
    }
}
