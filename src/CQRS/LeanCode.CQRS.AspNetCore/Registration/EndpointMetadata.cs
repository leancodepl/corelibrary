using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http.Metadata;

namespace LeanCode.CQRS.AspNetCore.Registration;

internal class EndpointTags : ITagsMetadata
{
    public IReadOnlyList<string> Tags { get; }

    public EndpointTags(IReadOnlyCollection<string> userTags, CQRSObjectMetadata metadata)
    {
        Tags = userTags.Concat(GetPredefinedTags(metadata)).ToList();
    }

    private static IEnumerable<string> GetPredefinedTags(CQRSObjectMetadata metadata)
    {
        yield return metadata.ObjectKind.ToString();
    }
}

internal class EndpointSummary(string summary) : IEndpointSummaryMetadata
{
    public string Summary => summary;
}

internal class EndpointDescription(string description) : IEndpointDescriptionMetadata
{
    public string Description => description;
}
