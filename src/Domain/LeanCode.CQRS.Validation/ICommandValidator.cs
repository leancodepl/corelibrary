using System.Threading.Tasks;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandValidator<in TAppContext, TContext, TCommand>
        where TCommand : ICommand<TContext>
    {
        Task<ValidationResult> ValidateAsync(
            TAppContext appContext, TContext context, TCommand command);
    }
}
