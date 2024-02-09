namespace LeanCode.CQRS.AspNetCore.Local;

public class UnauthorizedCQRSRequestException : Exception
{
    public Type ObjectType { get; }

    public UnauthorizedCQRSRequestException(Type objectType)
        : base($"The request {objectType.FullName} was not authorized.")
    {
        ObjectType = objectType;
    }
}
