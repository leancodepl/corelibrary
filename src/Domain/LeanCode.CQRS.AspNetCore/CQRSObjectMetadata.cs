namespace LeanCode.CQRS.AspNetCore;

public class CQRSObjectMetadata
{
    public CQRSObjectKind ObjectKind { get; }
    public Type ObjectType { get; }
    public Type ResultType { get; }
    public Type HandlerType { get; }

    internal CQRSObjectMetadata(CQRSObjectKind objectKind, Type objectType, Type resultType, Type handlerType)
    {
        ObjectKind = objectKind;
        ObjectType = objectType;
        ResultType = resultType;
        HandlerType = handlerType;
    }
}

public enum CQRSObjectKind
{
    Command,
    Query,
    Operation,
}
