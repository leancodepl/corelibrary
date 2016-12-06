namespace LeanCode.CQRS.Security
{
    public interface IAuthorizationChecker
    {
        bool CheckIfAuthorized<T>(T obj);
    }
}
