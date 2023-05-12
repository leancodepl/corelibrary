namespace LeanCode.CQRS.AspNetCore;

public delegate Task<object?> ObjectExecutor(IServiceProvider serviceProvider, CQRSRequestPayload payload);

public class CQRSEndpointMetadata
{
    public CQRSObjectKind ObjectKind { get; }
    public Type ObjectType { get; }
    public ObjectExecutor ObjectExecutor { get; }

    public CQRSEndpointMetadata(
        Type objectType,
        CQRSObjectKind objectKind,
        ObjectExecutor objectExecutor)
    {
        ObjectKind = objectKind;
        ObjectType = objectType;
        ObjectExecutor = objectExecutor;
    }
}

