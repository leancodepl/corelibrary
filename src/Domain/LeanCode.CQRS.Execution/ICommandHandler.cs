using System.Threading.Tasks;
using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Execution;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task ExecuteAsync(HttpContext context, TCommand command);
}
