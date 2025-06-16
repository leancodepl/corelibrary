using LeanCode.Contracts;

namespace LeanCode.AI.Contracts;

[ExcludeFromContractsGeneration]
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class McpToolAttribute(string description) : Attribute
{
    public string Description { get; } = description;
    public string? Name { get; init; }
    public string? Title { get; init; }
    public McpToolHints Hints { get; init; } = McpToolHints.Default;
}
