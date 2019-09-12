using System;
using System.Collections.Generic;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution.Simple
{
    public sealed class SimplePipelineContext : IEventsContext
    {
        private IPipelineScope? scope;

        public List<IDomainEvent> SavedEvents { get; set; } = new List<IDomainEvent>();
        public List<(IDomainEvent Event, Type Handler)> ExecutedHandlers { get; set; } = new List<(IDomainEvent Event, Type Handler)>();
        public List<(IDomainEvent Event, Type Handler)> FailedHandlers { get; set; } = new List<(IDomainEvent Event, Type Handler)>();

        public IPipelineScope Scope
        {
            get => scope ?? throw new NullReferenceException();
            set => scope = value;
        }
    }
}
