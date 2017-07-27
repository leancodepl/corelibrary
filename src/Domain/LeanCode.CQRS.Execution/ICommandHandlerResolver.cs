using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandHandlerResolver
    {
        ICommandHandlerWrapper FindCommandHandler(
            Type contextType, Type commandType);
    }

    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface ICommandHandlerWrapper
    {
        Task ExecuteAsync(object context, ICommand command);
    }
}
