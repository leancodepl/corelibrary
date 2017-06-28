namespace LeanCode.CQRS.Validation
{
    public interface ICommandValidatorResolver
    {
        ICommandValidator<TCommand> GetValidator<TCommand>()
            where TCommand : ICommand;
    }
}
