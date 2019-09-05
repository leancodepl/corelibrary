using System;
using System.Collections.Generic;
using LeanCode.Correlation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    public class Context : IEventsInterceptorContext, ICorrelationContext
    {
        public Guid CorrelationId { get; set; }
        public Guid ExecutionId { get; set; }
        public IPipelineScope Scope { get; set; }
        public List<IDomainEvent> SavedEvents { get; set; }
    }
}
