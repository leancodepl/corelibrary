using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface ICommandHandlerResolver
    {
        ICommandHandlerWrapper FindCommandHandler(Type commandType);
    }

    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface ICommandHandlerWrapper
    {
        Task ExecuteAsync(ICommand command);
    }
}
