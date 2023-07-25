using System.Text.Json.Serialization;
using LeanCode.DomainModels.Model;
using LeanCode.TimeProvider;

namespace LeanCode.IntegrationTests.App;

public class Entity
{
    public Guid Id { get; set; }
    public string Value { get; set; } = null!;
}

public class EntityAdded : IDomainEvent
{
    public Guid Id { get; private init; }
    public DateTime DateOccurred { get; private init; }

    public string Value { get; private init; }
    public Guid EntityId { get; private init; }

    [JsonConstructor]
    public EntityAdded(Guid id, DateTime dateOccurred, string value, Guid entityId)
    {
        Id = id;
        DateOccurred = dateOccurred;
        Value = value;
        EntityId = entityId;
    }

    public EntityAdded(Entity entity)
    {
        Id = Guid.NewGuid();
        DateOccurred = Time.UtcNow;

        EntityId = entity.Id;
        Value = entity.Value;
    }
}
