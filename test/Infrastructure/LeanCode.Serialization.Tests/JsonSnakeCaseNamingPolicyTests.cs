using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.Serialization.Tests
{
    public class JsonSnakeCaseNamingPolicyTests
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
        };

        [Fact]
        public void Correctly_converts_property_names()
        {
            var payload = new
            {
                snake_case = 0,
                camelCase = 1,
                PascalCase = 2,
                Darwin_Case = 3,
                MACRO_CASE = 4,
            };

            var json = JsonSerializer.Serialize(payload, SerializerOptions);

            var expectedJson = @"{""snake_case"":0,""camel_case"":1,""pascal_case"":2,""darwin_case"":3,""macro_case"":4}";

            Assert.Equal(expectedJson, json);
        }
    }
}
