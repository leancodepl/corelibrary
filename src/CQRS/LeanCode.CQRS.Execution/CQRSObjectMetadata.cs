using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Execution;

public delegate Task<object?> ObjectExecutor(HttpContext httpContext, CQRSRequestPayload payload);

public class CQRSObjectMetadata
{
    public CQRSObjectKind ObjectKind { get; }
    public Type ObjectType { get; }
    public Type ResultType { get; }
    public Type HandlerType { get; }
    public ObjectExecutor ObjectExecutor { get; }

    public CQRSObjectMetadata(
        CQRSObjectKind objectKind,
        Type objectType,
        Type resultType,
        Type handlerType,
        ObjectExecutor objectExecutor
    )
    {
        ObjectKind = objectKind;
        ObjectType = objectType;
        ResultType = resultType;
        HandlerType = handlerType;
        ObjectExecutor = objectExecutor;
    }
}

public class CQRSObjectMetadataEqualityComparer : IEqualityComparer<CQRSObjectMetadata>
{
    public bool Equals(CQRSObjectMetadata? x, CQRSObjectMetadata? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.ObjectType == y.ObjectType;
    }

    public int GetHashCode(CQRSObjectMetadata obj)
    {
        return HashCode.Combine(obj.ObjectKind, obj.ObjectType, obj.ResultType, obj.HandlerType);
    }
}

public enum CQRSObjectKind
{
    Command,
    Query,
    Operation,
}
