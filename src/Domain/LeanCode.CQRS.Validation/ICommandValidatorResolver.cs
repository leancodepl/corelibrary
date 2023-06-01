using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Validation;

public interface ICommandValidatorResolver
{
    ICommandValidatorWrapper? FindCommandValidator(Type commandType);
}

/// <summary>
/// Marker interface, do not use directly.
/// </summary>
public interface ICommandValidatorWrapper
{
    Task<ValidationResult> ValidateAsync(HttpContext appContext, ICommand command);
}
