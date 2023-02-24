using System.Collections.Concurrent;
using System.Text.Json;
using LeanCode.Components;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox;

/// <remarks>
/// `System.Text.Json` does not support private constructors and requires
/// users to add `JsonIncludeAttribute` to properties with private setters or exposing
/// constructors with all properties and marking it with `JsonConstructorAttribute`.
/// More info: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-immutability?pivots=dotnet-5-0
/// However, it is still more convenient than supporting different serializers.
/// </remarks>
public sealed class SystemTextJsonEventsSerializer : IRaisedEventsSerializer
{
    private readonly TypesCatalog typesCatalog;
    private readonly ConcurrentDictionary<string, Type> deserializerCache = new();
    private readonly JsonSerializerOptions options;

    public SystemTextJsonEventsSerializer(TypesCatalog typesCatalog, JsonSerializerOptions? options = null)
    {
        this.typesCatalog = typesCatalog;
        this.options = options ?? new();
    }

    public RaisedEvent WrapEvent(object evt, RaisedEventMetadata metadata)
    {
        return RaisedEvent.Create(evt, metadata, Serialize(evt));
    }

    public object ExtractEvent(RaisedEvent evt)
    {
        var type = deserializerCache.GetOrAdd(evt.EventType, GetEventType);
        return JsonSerializer.Deserialize(evt.Payload, type, options)!;
    }

    private string Serialize(object evt)
    {
        return JsonSerializer.Serialize(evt, options);
    }

    private Type GetEventType(string type)
    {
        return typesCatalog.Assemblies.Select(ass => ass.GetType(type)).FirstOrDefault(t => t != null)
            ?? throw new InvalidOperationException($"Type {type} is not defined in any of the provided assemblies.");
    }
}
