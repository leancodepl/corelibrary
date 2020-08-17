using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using LeanCode.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox
{
    public class NewtonsoftJsonEventsSerializer : IRaisedEventsSerializer
    {
        private readonly TypesCatalog typesCatalog;
        private readonly ConcurrentDictionary<string, Type> deserializerCache = new();
        private readonly JsonSerializerSettings settings;

        public NewtonsoftJsonEventsSerializer(TypesCatalog typesCatalog)
            : this(typesCatalog, null)
        { }

        public NewtonsoftJsonEventsSerializer(TypesCatalog typesCatalog, JsonSerializerSettings? settings)
        {
            this.typesCatalog = typesCatalog;
            this.settings = settings ?? new()
            {
                ContractResolver = new ContractResolver(),
            };
        }

        public RaisedEvent WrapEvent(object evt, RaisedEventMetadata metadata)
        {
            return RaisedEvent.Create(evt, metadata, JsonConvert.SerializeObject(evt,settings));
        }

        public object ExtractEvent(RaisedEvent evt)
        {
            var type = deserializerCache.GetOrAdd(evt.EventType, GetEventType);
            return JsonConvert.DeserializeObject(evt.Payload, type, settings)!;
        }

        private Type GetEventType(string type)
        {
            return typesCatalog.Assemblies
                .Select(ass => ass.GetType(type))
                .Where(t => t != null)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Type {type} is not defined in any of the provided assemblies.");
        }

        private class ContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
                => MakeWriteable(base.CreateProperty(member, memberSerialization), member);

            internal static JsonProperty MakeWriteable(JsonProperty jProperty, MemberInfo member)
            {
                if (jProperty.Writable)
                {
                    return jProperty;
                }
                else
                {
                    jProperty.Writable = member is PropertyInfo pi && pi.SetMethod is object;
                    return jProperty;
                }
            }
        }
    }
}
