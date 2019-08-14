using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LeanCode.Benchmarks
{
    public class InputToOutputMiddleware
    {
        private readonly JsonSerializer serializer = new JsonSerializer();

        public Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            object content;
            using (var reader = new JsonTextReader(new StreamReader(request.Body)))
            {
                content = serializer.Deserialize(reader);
            }

            response.ContentType = request.ContentType;
            response.StatusCode = 200;
            using (var writer = new JsonTextWriter(new StreamWriter(response.Body)))
            {
                serializer.Serialize(writer, content);
            }

            return Task.CompletedTask;
        }
    }
}
