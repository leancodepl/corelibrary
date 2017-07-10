using System.Threading.Tasks;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandInterceptor
    {
        Task<ValidationResult> InterceptAsync(ICommand command);
    }
}
