using Autofac;
using FluentValidation;
using LeanCode.CQRS;
using LeanCode.CQRS.FluentValidation;
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

    public class SampleCommandHandler : SyncCommandHandler<SampleCommand>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SampleCommandHandler>();

        private readonly ILifetimeScope scope;

        public SampleCommandHandler(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public override void Execute(SampleCommand command)
        {
            logger.Fatal("Tag: {Tag}, Hash: {Hash}, This: {This}", scope.Tag, scope.GetHashCode(), this.GetHashCode());

            DomainEvents.Raise(new SampleEvent());
        }
    }
}
