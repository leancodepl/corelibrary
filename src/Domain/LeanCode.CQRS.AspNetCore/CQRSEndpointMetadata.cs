namespace LeanCode.CQRS.AspNetCore;

public delegate Task<object?> ObjectExecutor(IServiceProvider serviceProvider, CQRSRequestPayload payload);

public class CQRSEndpointMetadata
{
    public CQRSObjectMetadata ObjectMetadata { get; }
    public ObjectExecutor ObjectExecutor { get; }

    public CQRSEndpointMetadata(CQRSObjectMetadata objectMetadata, ObjectExecutor objectExecutor)
    {
        ObjectMetadata = objectMetadata;
        ObjectExecutor = objectExecutor;
    }
}
