using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandHandlerResolver<TAppContext>
    {
        ICommandHandlerWrapper FindCommandHandler(Type commandType);
    }

    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface ICommandHandlerWrapper
    {
        Task ExecuteAsync(object context, ICommand command);
    }
}
