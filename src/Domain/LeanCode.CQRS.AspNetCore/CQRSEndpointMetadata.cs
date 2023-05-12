namespace LeanCode.CQRS.AspNetCore;

public delegate Task<object> ObjectExecutor(IServiceProvider serviceProvider, object context, object payload);

public class CQRSEndpointMetadata
{
    public CQRSObjectKind ObjectKind { get; }
    public Type ObjectType { get; }
    public Type ObjectHandlerType { get; }
    public ObjectExecutor ObjectExecutor { get; }

    public CQRSEndpointMetadata(
        Type objectType,
        CQRSObjectKind objectKind,
        Type objectHandlerType,
        ObjectExecutor objectExecutor)
    {
        ObjectKind = objectKind;
        ObjectType = objectType;
        ObjectHandlerType = objectHandlerType;
        ObjectExecutor = objectExecutor;
    }
}

public enum CQRSObjectKind
{
    Command,
    Query,
    Operation,
}

