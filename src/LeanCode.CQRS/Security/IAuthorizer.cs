namespace LeanCode.CQRS.Security
{
    public interface IAuthorizer
    {
        bool CheckIfAuthorized<T>(T obj);
    }
}
