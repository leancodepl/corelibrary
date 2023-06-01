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

public enum CQRSObjectKind
{
    Command,
    Query,
    Operation,
}
