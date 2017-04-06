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
        public override void Execute(SampleCommand command)
        {
            DomainEvents.Raise(new SampleEvent());
        }
    }
}
