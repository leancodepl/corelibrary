using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.DomainModels.Model;

namespace LeanCode.Example.CQRS
{
    public class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator()
        {
            RuleFor(c => c.Name).NotNull().NotEmpty().WithCode(1).WithMessage("Name must not be empty");
        }
    }

    public class SampleCommandHandler : ICommandHandler<AppContext, SampleCommand>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SampleCommandHandler>();

        private readonly ILifetimeScope scope;

        public SampleCommandHandler(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public Task ExecuteAsync(AppContext context, SampleCommand command)
        {
            logger.Information("Sample command called with data:");
            logger.Information("\tUserId (context): {UserId}", context.UserId);
            logger.Information("\tHeader (context): {Header}", context.Header);
            logger.Information("\tName (command): {Name}", command.Name);
            DomainEvents.Raise(new SampleEvent());
            return Task.CompletedTask;
        }
    }
}
