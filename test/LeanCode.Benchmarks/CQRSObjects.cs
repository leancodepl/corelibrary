using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using LeanCode.Components;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.Pipelines;

namespace LeanCode.Benchmarks
{
    public class SampleDTO { }
    public class SampleObjContext { }
    public class MultipleFieldsDTO
    {
        public string F1 { get; set; }
        public int F2 { get; set; }
    }

    public class PlainQuery : IRemoteQuery<SampleObjContext, SampleDTO> { }
    [QueryCache(3600)]
    public class CachedQuery : IRemoteQuery<SampleObjContext, SampleDTO> { }
    [AuthorizeWhenHasAnyOf("user")]
    public class UserQuery : IRemoteQuery<SampleObjContext, SampleDTO> { }
    [AuthorizeWhenHasAnyOf("admin")]
    public class AdminQuery : IRemoteQuery<SampleObjContext, SampleDTO> { }

    public class PlainCommand : IRemoteCommand<SampleObjContext> { }
    [AuthorizeWhenHasAnyOf("user")]
    public class UserCommand : IRemoteCommand<SampleObjContext> { }
    [AuthorizeWhenHasAnyOf("admin")]
    public class AdminCommand : IRemoteCommand<SampleObjContext> { }
    public class ValidCommand : IRemoteCommand<SampleObjContext> { }
    public class InvalidCommand : IRemoteCommand<SampleObjContext> { }

    public class MultipleFieldsQuery : IRemoteQuery<SampleObjContext, MultipleFieldsDTO>
    {
        public string F1 { get; set; }
        public int F2 { get; set; }
    }

    public class MultipleFieldsCommand : IRemoteCommand<SampleObjContext>
    {
        public string F1 { get; set; }
        public int F2 { get; set; }
    }

    public class SampleAppContext : IPipelineContext, ISecurityContext
    {
        public IPipelineScope Scope { get; set; }

        public ClaimsPrincipal User { get; set; }
    }

    public class SampleContextFromAppContext : IObjectContextFromAppContextFactory<SampleAppContext, SampleObjContext>
    {
        public SampleObjContext Create(SampleAppContext appContext)
        {
            return new SampleObjContext();
        }
    }

    public class SampleQueryHandler
        : IQueryHandler<SampleObjContext, PlainQuery, SampleDTO>,
          IQueryHandler<SampleObjContext, CachedQuery, SampleDTO>,
          IQueryHandler<SampleObjContext, UserQuery, SampleDTO>,
          IQueryHandler<SampleObjContext, AdminQuery, SampleDTO>
    {
        private static readonly Task<SampleDTO> Result = Task.FromResult(new SampleDTO());
        public Task<SampleDTO> ExecuteAsync(SampleObjContext context, PlainQuery query) => Result;
        public Task<SampleDTO> ExecuteAsync(SampleObjContext context, UserQuery query) => Result;
        public Task<SampleDTO> ExecuteAsync(SampleObjContext context, AdminQuery query) => Result;
        public Task<SampleDTO> ExecuteAsync(SampleObjContext context, CachedQuery query) => Result;
    }

    public class MultipleFieldsQueryHandler
        : IQueryHandler<SampleObjContext, MultipleFieldsQuery, MultipleFieldsDTO>
    {
        public static readonly Task<MultipleFieldsDTO> Result = Task.FromResult(
            new MultipleFieldsDTO
            {
                F1 = "test field",
                F2 = 123
            });
        public Task<MultipleFieldsDTO> ExecuteAsync(SampleObjContext context, MultipleFieldsQuery query) => Result;
    }

    public class SampleCommandHandler
        : ICommandHandler<SampleObjContext, PlainCommand>,
          ICommandHandler<SampleObjContext, UserCommand>,
          ICommandHandler<SampleObjContext, AdminCommand>,
          ICommandHandler<SampleObjContext, ValidCommand>,
          ICommandHandler<SampleObjContext, InvalidCommand>
    {
        public Task ExecuteAsync(SampleObjContext context, PlainCommand command) => Task.CompletedTask;
        public Task ExecuteAsync(SampleObjContext context, UserCommand command) => Task.CompletedTask;
        public Task ExecuteAsync(SampleObjContext context, AdminCommand command) => Task.CompletedTask;
        public Task ExecuteAsync(SampleObjContext context, ValidCommand command) => Task.CompletedTask;
        public Task ExecuteAsync(SampleObjContext context, InvalidCommand command) => Task.CompletedTask;
    }

    public class MultipleFieldsCommandHandler
        : ICommandHandler<SampleObjContext, MultipleFieldsCommand>
    {
        public Task ExecuteAsync(SampleObjContext context, MultipleFieldsCommand command) => Task.CompletedTask;
    }

    public class ValidCommandValidator : AbstractValidator<ValidCommand> { }
    public class InvalidCommandValidator : AbstractValidator<InvalidCommand>
    {
        public InvalidCommandValidator() => RuleFor(c => c).Must(c => false);
    }

    public class AppRoles : IRoleRegistration
    {
        public IEnumerable<Role> Roles => new[]
        {
            new Role("user", "user")
        };
    }

    public class BenchmarkModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppRoles>().AsImplementedInterfaces();
        }
    }
}
