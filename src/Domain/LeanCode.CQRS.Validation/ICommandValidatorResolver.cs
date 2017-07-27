using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandValidatorResolver
    {
        ICommandValidatorWrapper FindCommandValidator(
            Type contextType, Type commandType);
    }

    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface ICommandValidatorWrapper
    {
        Task<ValidationResult> ValidateAsync(object context, ICommand command);
    }
}
