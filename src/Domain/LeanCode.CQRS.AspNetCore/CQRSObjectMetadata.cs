namespace LeanCode.CQRS.AspNetCore;

public class CQRSObjectMetadata
{
    public CQRSObjectKind ObjectKind { get; }
    public Type ObjectType { get; }
    public Type ResultType { get; }
    public Type HandlerType { get; }
    public Type ContextType { get; }

    internal CQRSObjectMetadata(
        CQRSObjectKind objectKind,
        Type objectType,
        Type resultType,
        Type handlerType,
        Type contextType
    )
    {
        ObjectKind = objectKind;
        ObjectType = objectType;
        ResultType = resultType;
        HandlerType = handlerType;
        ContextType = contextType;
    }
}

public enum CQRSObjectKind
{
    Command,
    Query,
    Operation,
}
