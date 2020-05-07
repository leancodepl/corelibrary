using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using LeanCode.Components;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox
{
    public interface IRaisedEventsSerializer
    {
        object ExtractEvent(RaisedEvent evt);
        RaisedEvent WrapEvent(object evt, Guid correlationId);
    }

    public class JsonEventsSerializer : IRaisedEventsSerializer
    {
        private readonly TypesCatalog typesCatalog;
        private readonly ConcurrentDictionary<string, Type> deserializerCache = new ConcurrentDictionary<string, Type>();

        public JsonEventsSerializer(TypesCatalog typesCatalog)
        {
            this.typesCatalog = typesCatalog;
        }

        public RaisedEvent WrapEvent(object evt, Guid correlationId)
        {
            return RaisedEvent.Create(evt, correlationId, Serialize);
        }

        public object ExtractEvent(RaisedEvent evt)
        {
            var type = deserializerCache.GetOrAdd(evt.EventType, GetEventType);
            return JsonSerializer.Deserialize(evt.Payload, type);
        }

        private string Serialize(object payload)
        {
            return JsonSerializer.Serialize(payload, payload.GetType());
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
