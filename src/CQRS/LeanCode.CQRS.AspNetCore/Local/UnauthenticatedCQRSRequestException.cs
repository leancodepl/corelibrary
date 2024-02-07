namespace LeanCode.CQRS.AspNetCore.Local;

public class UnauthenticatedCQRSRequestException : Exception
{
    public Type ObjectType { get; }

    public UnauthenticatedCQRSRequestException(Type objectType)
        : base($"The request {objectType.FullName} was not authenticated.")
    {
        ObjectType = objectType;
    }
}
