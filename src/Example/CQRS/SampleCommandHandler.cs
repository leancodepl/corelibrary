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

    public class SampleCommandHandler : ICommandHandler<VoidContext, SampleCommand>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SampleCommandHandler>();

        private readonly ILifetimeScope scope;

        public SampleCommandHandler(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public Task ExecuteAsync(VoidContext _, SampleCommand command)
        {
            logger.Fatal("Tag: {Tag}, Hash: {Hash}, This: {This}", scope.Tag, scope.GetHashCode(), this.GetHashCode());
            logger.Information("Name: {Name}", command.Name);

            DomainEvents.Raise(new SampleEvent());
            return Task.CompletedTask;
        }
    }
}
