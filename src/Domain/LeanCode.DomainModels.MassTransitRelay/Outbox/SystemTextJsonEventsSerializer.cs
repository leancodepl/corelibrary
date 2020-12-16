using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using LeanCode.Components;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox
{
    /// <remarks>
    /// This is not usable yet. `System.Text.Json` does not support private constructors and requires
    /// users to add `JsonIncludeAttribute` to properties with private setters.
    /// More info: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-immutability?pivots=dotnet-5-0
    /// TODO: try to fix in .NET 6
    /// </remarks>
    internal sealed class SystemTextJsonEventsSerializer : IRaisedEventsSerializer
    {
        private readonly TypesCatalog typesCatalog;
        private readonly ConcurrentDictionary<string, Type> deserializerCache = new();
        private readonly JsonSerializerOptions options;

        public SystemTextJsonEventsSerializer(TypesCatalog typesCatalog, JsonSerializerOptions? options = null)
        {
            this.typesCatalog = typesCatalog;
            this.options = options ?? new JsonSerializerOptions();
        }

        public RaisedEvent WrapEvent(object evt, Guid correlationId)
        {
            return RaisedEvent.Create(evt, correlationId, Serialize(evt));
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
            return typesCatalog.Assemblies
                .Select(ass => ass.GetType(type))
                .Where(t => t != null)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Type: {type} is not defined in any of contract assemblies");
        }
    }
}
