#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using LeanCode.Pipelines;
using LeanCode.CQRS.Security;
using LeanCode.DomainModels.Model;
using LeanCode.DomainModels.EventsExecution;

namespace LeanCode.Example
{
    public sealed class AppContext : IEventsContext, ISecurityContext
    {
        private IPipelineScope? scope;
        private ClaimsPrincipal? user;

        List<IDomainEvent> IEventsContext.SavedEvents { get; set; } = new List<IDomainEvent>();
        List<(IDomainEvent Event, Type Handler)> IEventsContext.ExecutedHandlers { get; set; } = new List<(IDomainEvent Event, Type Handler)>();
        List<(IDomainEvent Event, Type Handler)> IEventsContext.FailedHandlers { get; set; } = new List<(IDomainEvent Event, Type Handler)>();

        IPipelineScope IPipelineContext.Scope
        {
            get => scope ?? throw new NullReferenceException();
            set => scope = value;
        }

        public ClaimsPrincipal User
        {
            get => user ?? throw new NullReferenceException();
            set => user = value;
        }

        public Guid UserId { get; }
        public string Header { get; }

        public AppContext(Guid userId, string header)
        {
            UserId = userId;
            Header = header;
        }

        public static AppContext FromHttp(HttpContext context)
        {
            Guid.TryParse(context.User?.FindFirst("sub")?.Value, out var uid);
            var header = context.Request.Headers["X-Example"];
            return new AppContext(uid, header);
        }
    }
}
