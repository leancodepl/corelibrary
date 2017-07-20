namespace LeanCode.CQRS.MvcValidation
{
    public interface ICommandResultTranslatorResolver
    {
        ICommandResultTranslator<TCommand> Resolve<TCommand>()
            where TCommand : ICommand;
    }
}
