namespace LeanCode.CQRS.AspNetCore;

public class CQRSObjectMetadata
{
    public Type ObjectType { get; }
    public CQRSObjectKind ObjectKind { get; }

    public CQRSObjectMetadata(Type objectType, CQRSObjectKind objectKind)
    {
        ObjectType = objectType;
        ObjectKind = objectKind;
    }
}

public enum CQRSObjectKind
{
    Command,
    Query,
    Operation,
}
