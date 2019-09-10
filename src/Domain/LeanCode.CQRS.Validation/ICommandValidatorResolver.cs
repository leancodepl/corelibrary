using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandValidatorResolver<TAppContext>
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
}
