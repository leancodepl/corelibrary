using System.Threading.Tasks;

namespace LeanCode.CQRS.Validation
{
    public interface ICommandInterceptor
    {
        Task<CommandResult> InterceptAsync(ICommand command);
    }
}
