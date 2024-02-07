namespace LeanCode.CQRS.AspNetCore.Local;

public class UnknownStatusCodeException : Exception
{
    public int StatusCode { get; }
    public Type ObjectType { get; }

    public UnknownStatusCodeException(int statusCode, Type objectType)
        : base($"Unknown status code {statusCode} for request {objectType.FullName}.")
    {
        StatusCode = statusCode;
        ObjectType = objectType;
    }
}
