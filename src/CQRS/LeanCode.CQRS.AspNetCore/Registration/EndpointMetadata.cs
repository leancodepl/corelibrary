using Microsoft.AspNetCore.Http.Metadata;

namespace LeanCode.CQRS.AspNetCore.Registration;

internal class EndpointTags(IReadOnlyList<string> tags) : ITagsMetadata
{
    public IReadOnlyList<string> Tags => tags;
}

internal class EndpointSummary(string summary) : IEndpointSummaryMetadata
{
    public string Summary => summary;
}

internal class EndpointDescription(string description) : IEndpointDescriptionMetadata
{
    public string Description => description;
}
