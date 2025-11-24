namespace LeanCode.CQRS.Execution;

/// <remarks>
/// May be registered multiple times.
/// </remarks>
public interface ICQRSEndpointMetadataProvider
{
    IEnumerable<object> GetAdditionalEndpointMetadata(CQRSObjectMetadata metadata);
}
