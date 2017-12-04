using System;
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

    public class ObjContext { }

    public class SampleCommand : ICommand<ObjContext>, IAuthorizerData
    { }

    public class SampleQuery : IQuery<ObjContext, object>, IAuthorizerData
    { }

    public class SampleCommandHandler : ICommandHandler<ObjContext, SampleCommand>
    {
        public static readonly AsyncLocal<SampleCommandHandler> LastInstance = new AsyncLocal<SampleCommandHandler>();

        public ObjContext Context { get; private set; }
        public SampleCommand Command { get; private set; }

        public SampleCommandHandler()
        {
            // Hacky! :D
            LastInstance.Value = this;
        }

        public Task ExecuteAsync(ObjContext context, SampleCommand command)
        {
            Context = context;
            Command = command;

            return Task.CompletedTask;
        }
    }

    public class SampleQueryHandler : IQueryHandler<ObjContext, SampleQuery, object>
    {
        public static readonly AsyncLocal<SampleQueryHandler> LastInstance = new AsyncLocal<SampleQueryHandler>();

        public ObjContext Context { get; private set; }
        public SampleQuery Query { get; private set; }
        public object Result { get; set; }

        public SampleQueryHandler()
        {
            LastInstance.Value = this;
        }

        public Task<object> ExecuteAsync(ObjContext context, SampleQuery query)
        {
            Context = context;
            Query = query;

            return Task.FromResult(Result);
        }
    }

    public class NoCHCommand : ICommand<ObjContext> { }
    public class NoQHQuery : IQuery<ObjContext, object> { }

    public interface HasSampleAuthorizer
    { }

    public interface IAuthorizerData { }

    public class SampleAuthorizer : CustomAuthorizer<AppContext, ObjContext, IAuthorizerData, object>, HasSampleAuthorizer
    {
        public static readonly AsyncLocal<SampleAuthorizer> LastInstance = new AsyncLocal<SampleAuthorizer>();

        public AppContext AppContext { get; set; }
        public ObjContext Context { get; private set; }
        public IAuthorizerData Object { get; private set; }
        public object Data { get; private set; }
        public bool Result { get; set; }

        public SampleAuthorizer()
        {
            LastInstance.Value = this;
        }

        protected override Task<bool> CheckIfAuthorizedAsync(
            AppContext appContext,
            ObjContext objContext,
            IAuthorizerData obj,
            object additionalData)
        {
            AppContext = appContext;
            Context = objContext;
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

    public class SampleValidator : ICommandValidator<AppContext, ObjContext, SampleCommand>
    {
        public static readonly AsyncLocal<SampleValidator> LastInstance = new AsyncLocal<SampleValidator>();

        public AppContext AppContext { get; private set; }
        public ObjContext Context { get; private set; }
        public SampleCommand Command { get; private set; }

        public ValidationResult Result { get; set; } = new ValidationResult(new ValidationError[0]);

        public SampleValidator()
        {
            LastInstance.Value = this;
        }

        public Task<ValidationResult> ValidateAsync(AppContext appContext, ObjContext context, SampleCommand command)
        {
            AppContext = appContext;
            Context = context;
            Command = command;

            return Task.FromResult(Result);
        }
    }

    public class SingleInstanceCommand : ICommand<ObjContext>
    { }

    public class SingleInstanceCommandHandler : ICommandHandler<ObjContext, SingleInstanceCommand>
    {
        public ObjContext Context { get; private set; }
        public SingleInstanceCommand Command { get; private set; }

        public Task ExecuteAsync(ObjContext context, SingleInstanceCommand command)
        {
            Context = context;
            Command = command;

            return Task.CompletedTask;
        }
    }

    class SamplePipelineElement<TObj, TRes> : IPipelineElement<AppContext, TObj, TRes>
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
