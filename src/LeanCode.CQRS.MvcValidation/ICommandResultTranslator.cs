using System;

namespace LeanCode.CQRS.MvcValidation
{
    public interface ICommandResultTranslator<TCommand>
        where TCommand : ICommand
    {
        string TranslateProperty(string name);
        string Translate(TCommand command, int code);
        bool CanHandle(TCommand command, Exception ex);
        string Translate(TCommand command, Exception ex);
    }
}
