using System;
using System.Collections.Concurrent;
using System.Linq;
using LeanCode.Components;
using Newtonsoft.Json;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox
{
    public interface IRaisedEventsSerializer
    {
        object ExtractEvent(RaisedEvent evt);
        RaisedEvent WrapEvent(object evt, Guid correlationId);
    }

    // https://github.com/dotnet/runtime/issues/29743
    // using JSON.NET as System.Text.Json doesn't support private setters (will be in .NET 5)
    // TODO: switch to System.Text.Json in .NET 5
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
            return RaisedEvent.Create(evt, correlationId, JsonConvert.SerializeObject);
        }

        public object ExtractEvent(RaisedEvent evt)
        {
            var type = deserializerCache.GetOrAdd(evt.EventType, GetEventType);
            return JsonConvert.DeserializeObject(evt.Payload, type);
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
