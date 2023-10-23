using System.Text.Json;

namespace LeanCode.AuditLogs;

public record EntityData(
    IReadOnlyList<string> Ids,
    string Type,
    JsonDocument Changes,
    JsonDocument ShadowProperties,
    string EntityState
);
