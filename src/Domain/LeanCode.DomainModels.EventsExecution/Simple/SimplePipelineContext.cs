using System;
using System.Collections.Generic;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution.Simple
{
    class SimplePipelineContext : IEventsContext
    {
        public List<IDomainEvent> SavedEvents { get; set; }
        public List<(IDomainEvent Event, Type Handler)> ExecutedHandlers { get; set; }
        public List<(IDomainEvent Event, Type Handler)> FailedHandlers { get; set; }
        public IPipelineScope Scope { get; set; }
    }
}
