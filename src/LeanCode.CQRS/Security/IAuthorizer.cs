using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public enum AuthorizationResult
    {
        Unauthenticated,
        InsufficientPermission,
        Authorized
    }

    public interface IAuthorizer
    {
        Task<AuthorizationResult> CheckIfAuthorized<T>(T obj);
    }
}
