using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.AuditLogs;

public static class ChangedEntitiesExtractor
{
    private static readonly JsonSerializerOptions Options =
        new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
        };

    public static IReadOnlyList<EntityData> Extract(DbContext dbContext)
    {
        return dbContext.ChangeTracker
            .Entries()
            .Where(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached)
            .Select(
                e =>
                    new EntityData(
                        // TODO: FIXME, I fail with owned entities
                        e.Metadata
                            .FindPrimaryKey()!
                            .Properties.Select(
                                p =>
                                    // This may lose some info comparing to JsonSerializer.Serialize , but we don't get
                                    // values in unnecessary "". We accept this tradeoff
                                    p.PropertyInfo?.GetValue(e.Entity, null)?.ToString()
                                    ?? "Cannot extract key property"
                            )
                            .ToList(),
                        e.Metadata.ClrType.ToString(),
                        JsonSerializer.SerializeToDocument(e.Entity, Options),
                        JsonSerializer.SerializeToDocument(
                            e.Properties
                                .Where(p => p.Metadata.IsShadowProperty())
                                .Select(
                                    p =>
                                        new
                                        {
                                            p.Metadata.Name,
                                            p.OriginalValue,
                                            p.CurrentValue
                                        }
                                ),
                            Options
                        ),
                        e.State.ToString()
                    )
            )
            .ToList();
    }
}
