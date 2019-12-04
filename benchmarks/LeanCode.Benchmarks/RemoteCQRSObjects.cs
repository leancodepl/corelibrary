using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LeanCode.Benchmarks
{
    public class InputToOutputMiddleware
    {
        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            response.ContentType = request.ContentType;
            response.StatusCode = 200;

            var content = await JsonSerializer.DeserializeAsync<MultipleFieldsQuery>(request.Body);
            await JsonSerializer.SerializeAsync(response.Body, content);
        }
    }
}
