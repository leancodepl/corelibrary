namespace LeanCode.CQRS.RemoteHttp.Server;

internal class CQRSEndpointMetadata
{
    public CQRSObjectMetadata ObjectMetadata { get; }
    public ObjectExecutor Executor { get; }

    public CQRSEndpointMetadata(CQRSObjectMetadata objectMetadata, ObjectExecutor executor)
    {
        ObjectMetadata = objectMetadata;
        Executor = executor;
    }
}
