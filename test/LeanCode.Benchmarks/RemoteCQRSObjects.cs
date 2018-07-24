using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LeanCode.Benchmarks
{
    public class InputToOutputMiddleware
    {
        private readonly JsonSerializer Serializer = new JsonSerializer();

        public Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            object content;
            using (var reader = new JsonTextReader(new StreamReader(request.Body)))
            {
                content = Serializer.Deserialize(reader);
            }

            response.ContentType = request.ContentType;
            response.StatusCode = 200;
            using (var writer = new JsonTextWriter(new StreamWriter(response.Body)))
            {
                Serializer.Serialize(writer, content);
            }
            return Task.CompletedTask;
        }
    }
}
