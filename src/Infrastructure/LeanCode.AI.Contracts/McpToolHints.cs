using LeanCode.Contracts;

namespace LeanCode.AI.Contracts;

[ExcludeFromContractsGeneration]
[Flags]
public enum McpToolHints
{
    None = 0,
    Default = 1,

    Destructive = 0b10,
    Idempotent = 0b100,
    OpenWorld = 0b1000,
    ReadOnly = 0b10000,
}
