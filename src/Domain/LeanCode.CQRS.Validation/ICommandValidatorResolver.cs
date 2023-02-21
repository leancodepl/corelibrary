using System;
using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;

namespace LeanCode.CQRS.Validation;

public interface ICommandValidatorResolver<in TAppContext>
    where TAppContext : notnull
{
    ICommandValidatorWrapper? FindCommandValidator(Type commandType);
}

/// <summary>
/// Marker interface, do not use directly.
/// </summary>
public interface ICommandValidatorWrapper
{
    Task<ValidationResult> ValidateAsync(object appContext, ICommand command);
}
