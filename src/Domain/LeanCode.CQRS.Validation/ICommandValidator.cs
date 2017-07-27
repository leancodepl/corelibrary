using System.Threading.Tasks;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandValidator<in TAppContext, TCommand>
        where TCommand : ICommand
    {
        Task<ValidationResult> ValidateAsync(
            TAppContext context, TCommand command);
    }
}
