namespace LeanCode.CQRS.Execution;

public class CQRSObjectMetadata
{
    public CQRSObjectKind ObjectKind { get; }
    public Type ObjectType { get; }
    public Type ResultType { get; }
    public Type HandlerType { get; }

    public CQRSObjectMetadata(CQRSObjectKind objectKind, Type objectType, Type resultType, Type handlerType)
    {
        ObjectKind = objectKind;
        ObjectType = objectType;
        ResultType = resultType;
        HandlerType = handlerType;
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

        return x.ObjectKind == y.ObjectKind
            && x.ObjectType == y.ObjectType
            && x.ResultType == y.ResultType
            && x.HandlerType == y.HandlerType;
    }

    public int GetHashCode(CQRSObjectMetadata obj)
    {
        return (obj.ObjectKind, obj.ObjectType, obj.ResultType, obj.HandlerType).GetHashCode();
    }
}

public enum CQRSObjectKind
{
    Command,
    Query,
    Operation,
}
