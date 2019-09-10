#nullable disable
// TODO: reenable when properties have been dealt with
using System;
using System.Collections.Generic;
using System.Security.Claims;
using LeanCode.CQRS.Security;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    public class DefaultExecutionContext : IEventsContext, ISecurityContext
    {
        public ClaimsPrincipal User { get; set; }

        List<IDomainEvent> IEventsContext.SavedEvents { get; set; }
        List<(IDomainEvent Event, Type Handler)> IEventsContext.ExecutedHandlers { get; set; }
        List<(IDomainEvent Event, Type Handler)> IEventsContext.FailedHandlers { get; set; }
        IPipelineScope IPipelineContext.Scope { get; set; }
    }
}
