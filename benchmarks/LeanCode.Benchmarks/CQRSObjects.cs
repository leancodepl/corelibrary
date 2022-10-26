using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.Pipelines;

namespace LeanCode.Benchmarks
{
    public class SampleDTO { }
    public class MultipleFieldsDTO
    {
        public string F1 { get; set; }
        public int F2 { get; set; }
    }

    public class PlainQuery : IQuery<SampleDTO> { }
    [AuthorizeWhenHasAnyOf("user")]
    public class UserQuery : IQuery<SampleDTO> { }
    [AuthorizeWhenHasAnyOf("admin")]
    public class AdminQuery : IQuery<SampleDTO> { }

    public class PlainCommand : ICommand { }
    [AuthorizeWhenHasAnyOf("user")]
    public class UserCommand : ICommand { }
    [AuthorizeWhenHasAnyOf("admin")]
    public class AdminCommand : ICommand { }
    public class ValidCommand : ICommand { }
    public class InvalidCommand : ICommand { }

    public class MultipleFieldsQuery : IQuery<MultipleFieldsDTO>
    {
        public string F1 { get; set; }
        public int F2 { get; set; }
    }

    public class MultipleFieldsCommand : ICommand
    {
        public string F1 { get; set; }
        public int F2 { get; set; }
    }

    public class SampleAppContext : IPipelineContext, ISecurityContext
    {
        public IPipelineScope Scope { get; set; }

        public ClaimsPrincipal User { get; set; }
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    }

    public class SampleQueryHandler
        : IQueryHandler<SampleAppContext, PlainQuery, SampleDTO>,
          IQueryHandler<SampleAppContext, UserQuery, SampleDTO>,
          IQueryHandler<SampleAppContext, AdminQuery, SampleDTO>
    {
        private static readonly Task<SampleDTO> Result = Task.FromResult(new SampleDTO());
        public Task<SampleDTO> ExecuteAsync(SampleAppContext context, PlainQuery query) => Result;
        public Task<SampleDTO> ExecuteAsync(SampleAppContext context, UserQuery query) => Result;
        public Task<SampleDTO> ExecuteAsync(SampleAppContext context, AdminQuery query) => Result;
    }

    public class MultipleFieldsQueryHandler
        : IQueryHandler<SampleAppContext, MultipleFieldsQuery, MultipleFieldsDTO>
    {
        public static readonly Task<MultipleFieldsDTO> Result = Task.FromResult(
            new MultipleFieldsDTO
            {
                F1 = "test field",
                F2 = 123,
            });
        public Task<MultipleFieldsDTO> ExecuteAsync(SampleAppContext context, MultipleFieldsQuery query) => Result;
    }

    public class SampleCommandHandler
        : ICommandHandler<SampleAppContext, PlainCommand>,
          ICommandHandler<SampleAppContext, UserCommand>,
          ICommandHandler<SampleAppContext, AdminCommand>,
          ICommandHandler<SampleAppContext, ValidCommand>,
          ICommandHandler<SampleAppContext, InvalidCommand>
    {
        public Task ExecuteAsync(SampleAppContext context, PlainCommand command) => Task.CompletedTask;
        public Task ExecuteAsync(SampleAppContext context, UserCommand command) => Task.CompletedTask;
        public Task ExecuteAsync(SampleAppContext context, AdminCommand command) => Task.CompletedTask;
        public Task ExecuteAsync(SampleAppContext context, ValidCommand command) => Task.CompletedTask;
        public Task ExecuteAsync(SampleAppContext context, InvalidCommand command) => Task.CompletedTask;
    }

    public class MultipleFieldsCommandHandler
        : ICommandHandler<SampleAppContext, MultipleFieldsCommand>
    {
        public Task ExecuteAsync(SampleAppContext context, MultipleFieldsCommand command) => Task.CompletedTask;
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
            new Role("user", "user"),
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
