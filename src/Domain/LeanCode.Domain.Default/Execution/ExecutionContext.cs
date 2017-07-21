using System;
using System.Collections.Generic;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.Domain.Default.Execution
{
    public struct ExecutionContext : IEventsContext
    {
        public IPipelineScope Scope { get; set; }
        public List<IDomainEvent> SavedEvents { get; set; }
        public List<(IDomainEvent Event, Type Handler)> ExecutedHandlers { get; set; }
        public List<(IDomainEvent Event, Type Handler)> FailedHandlers { get; set; }
    }
}
