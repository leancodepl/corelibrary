using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeanCode.Serialization
{
    public class JsonContent : HttpContent
    {
        private readonly object payload;
        private readonly Type type;
        private readonly JsonSerializerOptions? options;

        public JsonContent(object payload, Type type, JsonSerializerOptions? options)
        {
            this.payload = payload;
            this.type = type;
            this.options = options;

            Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = Encoding.UTF8.WebName,
            };
        }

        public JsonContent(object payload, Type type)
            : this(payload, type, null)
        { }

        public static JsonContent Create<T>(T payload)
            where T : notnull
        {
            return Create<T>(payload, null);
        }

        public static JsonContent Create<T>(T payload, JsonSerializerOptions? options)
            where T : notnull
        {
            return new JsonContent(payload, typeof(T), options);
        }

        public static JsonContent Create(object payload, Type type) =>
            Create(payload, type, null);

        public static JsonContent Create(object payload, Type type, JsonSerializerOptions? options) =>
            new JsonContent(payload, type, options);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) =>
            JsonSerializer.SerializeAsync(stream, payload, type, options);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
