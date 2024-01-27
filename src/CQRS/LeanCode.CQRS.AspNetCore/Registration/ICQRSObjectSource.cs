using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.AspNetCore.Registration;

public interface ICQRSObjectSource
{
    CQRSObjectMetadata MetadataFor(Type type);
}
