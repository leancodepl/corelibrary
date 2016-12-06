namespace LeanCode.CQRS.Validation
{
    public interface ICommandValidator<TCommand>
        where TCommand : ICommand
    {
        ValidationResult Validate(TCommand command);
    }
}
