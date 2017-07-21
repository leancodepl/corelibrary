using System;
using System.Collections.Generic;
using System.Security.Claims;
using LeanCode.CQRS.Security;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    public sealed class ExecutionContext : IEventsContext, ISecurityContext
    {
        public IPipelineScope Scope { get; set; }
        public List<IDomainEvent> SavedEvents { get; set; }
        public List<(IDomainEvent Event, Type Handler)> ExecutedHandlers { get; set; }
        public List<(IDomainEvent Event, Type Handler)> FailedHandlers { get; set; }
        public ClaimsPrincipal User { get; set; }
    }
}
