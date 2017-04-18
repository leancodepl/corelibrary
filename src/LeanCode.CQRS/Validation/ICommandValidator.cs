using System.Threading.Tasks;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandValidator<TCommand>
        where TCommand : ICommand
    {
        Task<ValidationResult> ValidateAsync(TCommand command);
    }
}
