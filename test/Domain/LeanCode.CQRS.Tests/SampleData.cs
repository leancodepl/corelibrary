using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Tests
{
    public class AppContext : IPipelineContext
    {
        IPipelineScope IPipelineContext.Scope { get; set; }
    }

    public class SampleCommand : ICommand, IAuthorizerData { }

    public class SampleQuery : IQuery<object>, IAuthorizerData { }

    public class SampleCommandHandler : ICommandHandler<AppContext, SampleCommand>
    {
        public static readonly AsyncLocal<SampleCommandHandler> LastInstance = new AsyncLocal<SampleCommandHandler>();

        public AppContext Context { get; private set; }
        public SampleCommand Command { get; private set; }

        public SampleCommandHandler()
        {
            // Hacky! :D
            LastInstance.Value = this;
        }

        public Task ExecuteAsync(AppContext context, SampleCommand command)
        {
            Context = context;
            Command = command;

            return Task.CompletedTask;
        }
    }

    public class SampleQueryHandler : IQueryHandler<AppContext, SampleQuery, object>
    {
        public static readonly AsyncLocal<SampleQueryHandler> LastInstance = new AsyncLocal<SampleQueryHandler>();

        public AppContext Context { get; private set; }
        public SampleQuery Query { get; private set; }
        public object Result { get; set; }

        public SampleQueryHandler()
        {
            LastInstance.Value = this;
        }

        public Task<object> ExecuteAsync(AppContext context, SampleQuery query)
        {
            Context = context;
            Query = query;

            return Task.FromResult(Result);
        }
    }

    public class NoCHCommand : ICommand { }
    public class NoQHQuery : IQuery<object> { }

    [SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1302:InterfaceNamesMustBeginWithI",
        Justification = "Authorizers are exempt.")]
    public interface HasSampleAuthorizer { }

    public interface IAuthorizerData { }

    public class SampleAuthorizer : CustomAuthorizer<AppContext, IAuthorizerData, object>, HasSampleAuthorizer
    {
        public static readonly AsyncLocal<SampleAuthorizer> LastInstance = new AsyncLocal<SampleAuthorizer>();

        public AppContext AppContext { get; set; }
        public IAuthorizerData Object { get; private set; }
        public object Data { get; private set; }
        public bool Result { get; set; }

        public SampleAuthorizer()
        {
            LastInstance.Value = this;
        }

        protected override Task<bool> CheckIfAuthorizedAsync(
            AppContext appContext,
            IAuthorizerData obj,
            object additionalData)
        {
            AppContext = appContext;
            Object = obj;
            Data = additionalData;
            return Task.FromResult(Result);
        }
    }

    public class SampleEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime DateOccurred { get; } = DateTime.UtcNow;
    }

    public class SampleEvent2 : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime DateOccurred { get; } = DateTime.UtcNow;
    }

    public class SampleEventHandler : IDomainEventHandler<SampleEvent>
    {
        public static readonly AsyncLocal<SampleEventHandler> LastInstance = new AsyncLocal<SampleEventHandler>();

        public SampleEvent Event { get; private set; }

        public SampleEventHandler()
        {
            LastInstance.Value = this;
        }

        public Task HandleAsync(SampleEvent domainEvent)
        {
            Event = domainEvent;
            return Task.CompletedTask;
        }
    }

    public class SampleValidator : ICommandValidator<AppContext, SampleCommand>
    {
        public static readonly AsyncLocal<SampleValidator> LastInstance = new AsyncLocal<SampleValidator>();

        public AppContext AppContext { get; private set; }
        public SampleCommand Command { get; private set; }

        public ValidationResult Result { get; set; } = new ValidationResult(new ValidationError[0]);

        public SampleValidator()
        {
            LastInstance.Value = this;
        }

        public Task<ValidationResult> ValidateAsync(AppContext appContext, SampleCommand command)
        {
            AppContext = appContext;
            Command = command;

            return Task.FromResult(Result);
        }
    }

    public class SingleInstanceCommand : ICommand { }

    public class SingleInstanceCommandHandler : ICommandHandler<AppContext, SingleInstanceCommand>
    {
        public AppContext Context { get; private set; }
        public SingleInstanceCommand Command { get; private set; }

        public Task ExecuteAsync(AppContext context, SingleInstanceCommand command)
        {
            Context = context;
            Command = command;

            return Task.CompletedTask;
        }
    }

    public class SingleInstanceQuery : IQuery<object> { }

    public class SingleInstanceQueryHandler : IQueryHandler<AppContext, SingleInstanceQuery, object>
    {
        public AppContext Context { get; private set; }
        public SingleInstanceQuery Query { get; private set; }
        public object Result { get; set; }

        public Task<object> ExecuteAsync(AppContext context, SingleInstanceQuery query)
        {
            Context = context;
            Query = query;
            return Task.FromResult(Result);
        }
    }

    internal class SamplePipelineElement<TObj, TRes> : IPipelineElement<AppContext, TObj, TRes>
    {
        public AppContext AppContext { get; set; }
        public TObj Data { get; set; }

        public Task<TRes> ExecuteAsync(AppContext ctx, TObj input, Func<AppContext, TObj, Task<TRes>> next)
        {
            AppContext = ctx;
            Data = input;
            return next(ctx, input);
        }
    }
}
