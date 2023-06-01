using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Validation;

public interface ICommandValidator<TCommand>
    where TCommand : ICommand
{
    Task<ValidationResult> ValidateAsync(HttpContext httpContext, TCommand command);
}
